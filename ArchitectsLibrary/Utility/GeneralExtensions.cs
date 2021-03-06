using System;
using UnityEngine;

namespace ArchitectsLibrary.Utility
{
    /// <summary>
    /// a Class that contains a collection of useful extensions.
    /// </summary>
    public static class GeneralExtensions
    {
        /// <summary>
        /// Disables the <see cref="Collider"/>s and <see cref="Renderer"/>s of a <see cref="GameObject"/> without making it completely In Active.
        /// </summary>
        /// <param name="gameObject"></param>
        public static void SemiInActive(this GameObject gameObject)
        {
            foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
                collider.enabled = false;

            foreach (var renderer in gameObject.GetAllComponentsInChildren<Renderer>())
                renderer.enabled = false;
        }
		
		/// <summary>
		/// An extension for adding an item to an array.
		/// </summary>
		/// <param name="array">the array to add to.</param>
		/// <param name="item">item to add to the array.</param>
		public static void Add<T>(this T[] array, T item)
		{
			Array.Resize<T>(ref array, array.Length + 1);
			array[array.Length - 1] = item;
		}

        /// <summary>
        /// searches for a Child Object of a <see cref="GameObject"/> by passing the Child's name.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="byName">The Child to search for.</param>
        /// <returns>Found Child from the passed string.</returns>
        public static GameObject SearchChild(this GameObject gameObject, string byName)
        {
            GameObject obj = SearchChildRecursive(gameObject, byName);
            if (obj is null)
            {
                ErrorMessage.AddMessage($"No child found in hierarchy by name {byName}.");
            }
            return obj;
        }

        /// <summary>
        /// Destroy component of type <typeparamref name="T"/> if it exists in this object or its children.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive"></param>
        public static bool TryDestroyChildComponent<T>(this GameObject gameObject, bool includeInactive = true) where T : Component
        {
            T component = gameObject.GetComponentInChildren<T>(includeInactive);
            if(component is not null)
            {
                UnityEngine.Object.DestroyImmediate(component);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Destroys all component of type <typeparamref name="T"/> if they exist in this object or its children.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive"></param>
        public static void TryDestroyChildComponents<T>(this GameObject gameObject, bool includeInactive = true) where T : Component
        {
            T[] components = gameObject.GetComponentsInChildren<T>(includeInactive);
            if (components is not null)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(components[i]);
                }
            }
        }

        static GameObject SearchChildRecursive(GameObject gameObject, string byName)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (string.Equals(child.gameObject.name, byName))
                {
                    return child.gameObject;
                }
                GameObject recursive = SearchChildRecursive(child.gameObject, byName);
                if (recursive)
                {
                    return recursive;
                }
            }
            return null;
        }
    }
}