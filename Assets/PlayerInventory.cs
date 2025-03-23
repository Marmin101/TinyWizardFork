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
                Debug.Log("PENIS " + staff == null);
                Node newNode = new Node(staff);
                if (head == null)
                {
                    head = newNode;
                }
                else
                {
                    Node current = head;
                    while (current.next != null)
                    {
                        current = current.next;
                    }
                    current.next = newNode;
                }
            }
            public Staff FindNextStaff(Staff currentStaff)
            {
                
                Node current = head;

                while (current.staff != currentStaff)
                {
                    current = current.next;
                }
                print(current.staff);
                return(current.next != null ? current.next.staff : head.staff);
            }

            public void RemoveStaffFromList(Staff staffToRemove)
            {
                Node current = head;
                if (current.next != null)
                {
                    while (current.next.staff != staffToRemove)
                    {
                        current = current.next;
                    }
                    if (current.next.next != null)
                    {
                        current.next = current.next.next;
                    }
                    else
                    {
                        current.next = null;
                    }
                }
                else
                {
                    head = null;
                }

            }

            public void PrintList()
            {
                //Debug.Log(123);
                Node current = head;
                while (current != null)
                {
                    Debug.Log(current.staff);
                    current = current.next;
                }
                current = head;
            }

        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public Inventory inventory { get; private set; } = new Inventory();


        // Update is called once per frame
        void Update()
        {
        }
    }
}
