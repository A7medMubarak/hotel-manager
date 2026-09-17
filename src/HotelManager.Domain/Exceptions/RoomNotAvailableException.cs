namespace HotelManager.Domain.Exceptions;

public class RoomNotAvailableException : Exception
{
    public RoomNotAvailableException(string message) : base(message) { }
}
