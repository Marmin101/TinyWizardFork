namespace Quinn.UnityServices.Events
{
	public class EnterRoomEvent : CustomEvent
	{
		public int RoomsExploredThisFloor { set => SetParameter("roomsExplored", value); }

		public EnterRoomEvent()
			: base(Prefix + "enter_new_room") { }
	}
}
