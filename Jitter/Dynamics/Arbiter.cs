﻿/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

#region Using Statements
using System;
using System.Collections.Generic;

using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using System.Diagnostics;
#endregion

namespace Jitter.Dynamics
{
    /// <summary>
    /// Represents a list of contacts. Every ContactList 
    /// has a maximum of four contacts.
    /// </summary>
    public class ContactList : List<Contact>
    {
        public ContactList() : base(4) { }

        #region TODO: Write an implementation which only has 4 elements.

        //Contact[] contacts = new Contact[4];
        //int count = 0;

        //public void Add(Contact contact)
        //{
        //    contacts[count] = contact;
        //    count++;
        //}

        //public int Count { get { return count; } }

        //public Contact this[int index]
        //{
        //    get
        //    {
        //        return contacts[index];
        //    }
        //}

        //public void RemoveAt(int index)
        //{
        //    if (index == 2)
        //    {
        //        contacts[2] = contacts[3];
        //    }
        //    else if (index == 1)
        //    {
        //        contacts[1] = contacts[2];
        //        contacts[2] = contacts[3];
        //    }
        //    else if (index == 0)
        //    {
        //        contacts[0] = contacts[1];
        //        contacts[1] = contacts[2];
        //        contacts[2] = contacts[3];
        //    }

        //    count--;
        //}

        //public void Clear()
        //{
        //    count = 0;
        //}
        #endregion
    }

    /// <summary>
    /// An arbiter holds all contact information of two bodies.
    /// The contacts are stored in the ContactList. There is a maximum
    /// of four contacts which can be added to an arbiter. The arbiter
    /// only keeps the best four contacts based on the area spanned by
    /// the contact points.
    /// </summary>
    public class Arbiter
    {
        public static World World { get; set; }

        /// <summary>
        /// The first body.
        /// </summary>
        public RigidBody Body1 => body1;

        /// <summary>
        /// The second body.
        /// </summary>
        public RigidBody Body2 => body2;

        /// <summary>
        /// The contact list containing all contacts of both bodies.
        /// </summary>
        public ContactList ContactList => contactList;

        /// <summary>
        /// </summary>
        public static ResourcePool<Arbiter> Pool = new ResourcePool<Arbiter>();

        // internal values for faster access within the engine
        internal RigidBody body1, body2;
        internal ContactList contactList;

        /// <summary>
        /// </summary>
        /// <param name="body1"></param>
        /// <param name="body2"></param>
        public Arbiter(RigidBody body1, RigidBody body2)
        {
            this.contactList = new ContactList();
            this.body1 = body1;
            this.body2 = body2;
        }

        /// <summary>
        /// Initializes a new instance of the Arbiter class.
        /// </summary>
        public Arbiter()
        {
            this.contactList = new ContactList();
        }

        /// <summary>
        /// Removes all contacts from this arbiter.
        /// The world will remove the arbiter automatically next frame
        /// or add new contacts.
        /// </summary>
        public void Invalidate()
        {
            contactList.Clear();
        }

        /// <summary>
        /// Adds a contact to the arbiter (threadsafe). No more than four contacts 
        /// are stored in the contactList. When adding a new contact
        /// to the arbiter the existing are checked and the best are kept.
        /// </summary>
        /// <param name="point1">Point on body1. In world space.</param>
        /// <param name="point2">Point on body2. In world space.</param>
        /// <param name="normal">The normal pointing to body2.</param>
        /// <param name="penetration">The estimated penetration depth.</param>
        public void AddContact(JVector point1, JVector point2, JVector normal, JVector[] triangle,
            float penetration, ContactSettings contactSettings)
        {
            // Returning false means that the contact doesn't need to be kept (likely because data from the contact
            // was used to manually update controllers).
            if (!World.Events.RaiseContactCreated(body1, body2, point1, point2, normal, triangle, penetration))
            {
                return;
            }

            JVector.Subtract(ref point1, ref body1.position, out var relPos1);

            lock (contactList)
            {
                int index;

                if (contactList.Count == 4)
                {
                    index = SortCachedPoints(ref relPos1, penetration);
                    ReplaceContact(ref point1, ref point2, ref normal, triangle, penetration, index, contactSettings);

                    // TODO: I added this return (to prevent contact count blowing up). Is this correct?
                    return;
                }

                index = GetCacheEntry(ref relPos1, contactSettings.breakThreshold);

                if (index >= 0)
                {
                    ReplaceContact(ref point1, ref point2, ref normal, triangle, penetration, index, contactSettings);
                }

                Contact contact = Contact.Pool.GetNew();
                contact.Initialize(body1, body2, ref point1, ref point2, ref normal, triangle, penetration, true,
                    contactSettings);
                contactList.Add(contact);
            }
        }

        private void ReplaceContact(ref JVector point1, ref JVector point2, ref JVector n, JVector[] triangle, float p,
            int index, ContactSettings contactSettings)
        {
            Contact contact = contactList[index];

            Debug.Assert(body1 == contact.body1, "Body1 and Body2 not consistent.");

            contact.Initialize(body1, body2, ref point1, ref point2, ref n, triangle, p, false, contactSettings);
        }

        private int GetCacheEntry(ref JVector realRelPos1, float contactBreakThreshold)
        {
            float shortestDist = contactBreakThreshold * contactBreakThreshold;
            int size = contactList.Count;
            int nearestPoint = -1;
           
            for (int i = 0; i < size; i++)
            {
                JVector.Subtract(ref contactList[i].relativePos1,ref realRelPos1,out var diffA);
                float distToManiPoint = diffA.LengthSquared();
                
                if (distToManiPoint < shortestDist)
                {
                    shortestDist = distToManiPoint;
                    nearestPoint = i;
                }
            }

            return nearestPoint;
        }

        // sort cached points so most isolated points come first
        private int SortCachedPoints(ref JVector realRelPos1, float pen)
        {
            //calculate 4 possible cases areas, and take biggest area
            //also need to keep 'deepest'

            int maxPenetrationIndex = -1;
            float maxPenetration = pen;
            for (int i = 0; i < 4; i++)
            {
                if (contactList[i].penetration > maxPenetration)
                {
                    maxPenetrationIndex = i;
                    maxPenetration = contactList[i].penetration;
                }
            }

            float res0 = 0, res1 = 0, res2 = 0, res3 = 0;
            if (maxPenetrationIndex != 0)
            {
                JVector.Subtract(ref realRelPos1,ref contactList[1].relativePos1,out var a0);
                JVector.Subtract(ref contactList[3].relativePos1, ref contactList[2].relativePos1, out var b0);
                JVector.Cross(ref a0, ref b0, out var cross);
                res0 = cross.LengthSquared();
            }
            if (maxPenetrationIndex != 1)
            {
                JVector.Subtract(ref realRelPos1, ref contactList[0].relativePos1, out var a0);
                JVector.Subtract(ref contactList[3].relativePos1, ref contactList[2].relativePos1, out var b0);
                JVector.Cross(ref a0, ref b0, out var cross);
                res1 = cross.LengthSquared();
            }

            if (maxPenetrationIndex != 2)
            {
                JVector.Subtract(ref realRelPos1, ref contactList[0].relativePos1, out var a0);
                JVector.Subtract(ref contactList[3].relativePos1, ref contactList[1].relativePos1, out var b0);
                JVector.Cross(ref a0, ref b0, out var cross);
                res2 = cross.LengthSquared();
            }

            if (maxPenetrationIndex != 3)
            {
                JVector.Subtract(ref realRelPos1, ref contactList[0].relativePos1, out var a0);
                JVector.Subtract(ref contactList[2].relativePos1, ref contactList[1].relativePos1, out var b0);
                JVector.Cross(ref a0, ref b0, out var cross);
                res3 = cross.LengthSquared();
            }

            int biggestarea = MaxAxis(res0, res1, res2, res3);
            return biggestarea;
        }

        internal static int MaxAxis(float x, float y, float z, float w)
        {
            int maxIndex = -1;
            float maxVal = float.MinValue;

            if (x > maxVal) { maxIndex = 0; maxVal = x; }
            if (y > maxVal) { maxIndex = 1; maxVal = y; }
            if (z > maxVal) { maxIndex = 2; maxVal = z; }
            if (w > maxVal) { maxIndex = 3; maxVal = w; }

            return maxIndex;
        }

    }
}
