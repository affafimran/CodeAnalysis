using System.Collections.Generic;

namespace NullPointerGame.ResourceSystem
{
	/// <summary>
	/// Mask to be used as filter to define a list of allowed reesources.
	/// </summary>
	[System.Serializable]
	public class ResourcesMask
	{
		/// <summary>
		/// bitwise container for the mask.
		/// </summary>
		public int mask = 0;

		/// <summary>
		/// Indicates if the resource id its marked inthe mask.
		/// </summary>
		/// <param name="id">The index id of the resource to be checked.</param>
		/// <returns></returns>
		public bool Contains(int id)
		{
			return (mask & (1 << id))!= 0;
		}
		/// <summary>
		/// The integer mask value.
		/// </summary>
		public int Mask { get { return mask; } set { mask = value; } }
	}
}
