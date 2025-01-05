using System;

namespace KartLibrary.IO
{
	public sealed class PacketReadException : Exception
	{
		public PacketReadException(string message) : base(message)
		{
		}
	}
}