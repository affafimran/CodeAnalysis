using UnityEngine;

namespace NullPointerGame.ResourceSystem
{
	/// <summary>
	/// Resource id type to be used by the entire resource system.
	/// </summary>
	[System.Serializable]
	public sealed class ResourceID
	{
		[SerializeField]
		private int id=0;

		/// <summary>
		/// The converted integer value for this resource id.
		/// </summary>
		public int ID { get { return id; } set { id = value; }  }

		/// <summary>
		/// Default Resource id constructor.
		/// </summary>
		public ResourceID()
		{
			id = 0;
		}

		/// <summary>
		/// Resource id constructor taking an integer value as initial value.
		/// </summary>
		/// <param name="source">the initial id value.</param>
		public ResourceID(int source)
		{
			id = source;
		}

		/// <summary>
		/// Indicates if the ResourceID is an invalid id.
		/// </summary>
		/// <param name="rid"></param>
		/// <returns>true in case the rid is an invalid type; false in otherwise.</returns>
		public static bool IsInvalid(ResourceID rid)
		{
			return rid==null;
		}

		/// <summary>
		/// Converts an int value to a ResourceID type.
		/// </summary>
		/// <param name="source">integer value to be converted.</param>
		public static implicit operator ResourceID(int source)
		{
			return new ResourceID(source);
		}

		/// <summary>
		/// User-defined conversion from ResourceID to int
		/// </summary>
		/// <param name="source">value to be checked.</param>
		public static implicit operator int(ResourceID source)
		{
			if(source!=null)
				return source.id;
			return 0;
		}

		/// <summary>
		/// User-defined conversion from ResourceID to bool (used for content validation) 
		/// </summary>
		/// <param name="val">value to be checked.</param>
		public static implicit operator bool(ResourceID val)
		{
			return val!=null;
		}

		/// <summary>
		/// static equality comparer for a ResourceID tuple.
		/// </summary>
		/// <param name="val1">The 1st resource id to be compared.</param>
		/// <param name="val2">The 2nd resource id to be compared.</param>
		/// <returns>true if both references are not null and contains the same id.</returns>
		public static bool operator ==(ResourceID val1, ResourceID val2)
		{
			if (ReferenceEquals(null, val1)) return false;
			return val1.Equals(val2);
		}
		
		/// <summary>
		/// static inequality comparer for a ResourceID tuple.
		/// </summary>
		/// <param name="val1">The 1st resource id to be compared.</param>
		/// <param name="val2">The 2nd resource id to be compared.</param>
		/// <returns>true if both references are not null and contains diferent id.</returns>
		public static bool operator!= (ResourceID val1, ResourceID val2)
		{
			if (ReferenceEquals(null, val1)) return false;
			return !val1.Equals(val2);
		}

		/// <summary>
		/// Overrided method that implements the proper hash for this object.
		/// </summary>
		/// <returns>The hash code for the current ResourceID.</returns>
		public override int GetHashCode()
		{
			return id;
		}

		/// <summary>
		/// Overrided implementation that determines whether the specified ResourceID is equal to the current ResourceID.
		/// </summary>
		/// <param name="other">The ResourceID to compare with the current ResourceID.</param>
		/// <returns>true if the specified ResourceID is equal to the current ResourceID; otherwise, false.</returns>
		public bool Equals(ResourceID other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return id == other.id;
		}

		/// <summary>
		/// Overrided implementation that determines whether the specified object is equal to the current ResourceID.
		/// </summary>
		/// <param name="obj">The object to compare with the current ResourceID.</param>
		/// <returns>true if the specified object is equal to the current ResourceID; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (ResourceID)) return false;
			return Equals((ResourceID) obj);
		}

	}
}
