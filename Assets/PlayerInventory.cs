using Quinn.AI;
using Quinn.PlayerSystem;
using Quinn.PlayerSystem.SpellSystem;
using Quinn.PlayerSystem.SpellSystem.Staffs;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

namespace Quinn
{
    public class PlayerInventory : MonoBehaviour
    {

        //define what a node contains
        public class Node
        {
            public Staff staff;
            public Node next;

            public Node(Staff staff)
            {
                this.staff = staff;
                this.next = null;
            }
        }

        public class Inventory
        {
            //public Staff currentlyEquipped = null;
            public Node head;
            public void AddToInventory(Staff staff)
            {
                //Initialize the new node being added
                Node newNode = new Node(staff);
                if (head == null)
                {
                    //if there is no list, the new node is the new head
                    head = newNode;
                }
                else
                {
                    //start at the head
                    Node current = head;
                    while (current.next != null)
                    {
                        //while the next node exists, keep going
                        current = current.next;
                    }
                    //when the next node is null, set it to the new node we created
                    current.next = newNode;
                }
            }
            public Staff FindNextStaff(Staff currentStaff)
            {
                //start at the head
                Node current = head;

                //if the current staff is not the same as the staff we currently have keep going through the list
                while (current.staff != currentStaff)
                {
                    current = current.next;
                }
                //if the current staff is not the last element, return the one after it, otherwise, return the first element in the list.
                //this makes sure the inventory loops instead of raising an error.
                return(current.next != null ? current.next.staff : head.staff);
            }

            public void RemoveStaffFromList(Staff staffToRemove)
            {
                //start at the head
                Node current = head;

                //if we arent at the end of the list
                if (current.next != null)
                {
                    //traverse through the list until we land on the one before the staff to remove
                    while (current.next.staff != staffToRemove)
                    {
                        current = current.next;
                    }
                    //if the element after the next isnt null, make the next element the one after the next, meaning we skip one
                    if (current.next.next != null)
                    {
                        current.next = current.next.next;
                    }
                    //if the element was null, set the next to null removing from the end.
                    else
                    {
                        current.next = null;
                    }
                }
                //if we are removing the first element, set the head to null
                else
                {
                    head = null;
                }

            }
            //search for an item in my list and return true if it is found
            public bool SearchForItem(Staff searchElement)
            {
                //start at the head
                Node current = head;

                //search the whole list
                while (current != null)
                {
                    //if it is found, return true
                    if(current.staff == searchElement)
                    {
                        return true;
                    }
                }
                //if we reach the end of the list, we return false
                return false;
            }

        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public Inventory inventory { get; private set; } = new Inventory();
    }
}
