﻿using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
	/// <summary>
	/// Base class for all random props
	/// </summary>
	public abstract class RandomProp : MonoBehaviour
	{
		/// <summary>
		/// Process the random prop. DunGen will call this method on any GameObject in the dungeon
		/// </summary>
		/// <param name="randomStream">Seeded random number generator</param>
		/// <param name="tile">Owning tile</param>
		/// <param name="spawnedObjects">An output list of objects that have been spawned in this function, to be passed back to DunGen for further processing</param>
		public abstract void Process(RandomStream randomStream, Tile tile, ref List<GameObject> spawnedObjects);
	}
}
