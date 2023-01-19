using System.Collections.Generic;
using System;

namespace GPL
{
    /// <summary>
    /// Offers a custom pool for an object of a given `Type`.
    /// 
    /// The pool has several functions
    /// - First receives a default template for an object.
    /// - It can automatically create copies of the default template.
    /// - Makes sure there is always at least 1 `Type` object in the queue, ready to be used.
    /// - Explicit Capacity Set
    /// - Functionality for clearing the Pooling Object
    /// - more...
    /// </summary>
    /// <typeparam name="Type">Type of Object to store in the pool</typeparam>
    public class ObjectPooling<Type>
    {
        /// <summary>
        /// Provides the way how the object of type `Type`
        /// can be duplicated. Useful because the pool automatically creates 
        /// copies when needed
        /// </summary>
        public interface ObjectInstantiatorService
        {
            /// <summary>
            /// Instantiate a clone of T Object.
            /// </summary>
            /// <param name="t"></param>
            /// <returns>The new clone of T</returns>
            Type Instantiate(Type t);
        }

        /// <summary>
        /// All the stored objects that can be used.
        /// </summary>
        protected List<Type> storedObjects;

        /// <summary>
        /// The default object. All other objects are a copy of this object.
        /// </summary>
        public Type DefaultObject
        {
            get { return defaultObject; }
            set { defaultObject = value; }
        }
        protected Type defaultObject;

        /// <summary>
        /// The remaining objects in the pool
        /// </summary>
        public int RemainingObjects
        {
            get { return storedObjects.Count; }
        }

        /// <summary>
        /// Contains the needed method to instantiate/clone objects
        /// </summary>
        private ObjectInstantiatorService instantiationProvider;


        /// <param name="defaultObject"></param>
        /// <param name="instantiationProvider"></param>
        /// <param name="capacity">How many objects to store MAX</param>
        public ObjectPooling(Type defaultObject, ObjectInstantiatorService instantiationProvider, int capacity)
        {
            this.instantiationProvider = instantiationProvider;
            this.DefaultObject = defaultObject;
            this.storedObjects = new List<Type>(capacity);
        }

        /// <param name="defaultObject"></param>
        /// <param name="instantiationProvider"></param>
        /// <param name="capacity">How many objects to store MAX</param>
        /// <param name="initialObjects">How many objects to fill at start</param>
        public ObjectPooling(Type defaultObject, ObjectInstantiatorService instantiationProvider, int capacity, int initialObjects)
        {
            this.instantiationProvider = instantiationProvider;
            this.DefaultObject = defaultObject;
            this.storedObjects = new List<Type>(capacity);
            StoreObjects(initialObjects);
        }


        /// <summary>
        /// Set the capacity of the Storage.
        /// </summary>
        /// 
        /// \warning    The capacity needs to be higher than the current amount of elements.
        /// 
        /// <exception cref="ArgumentException">If the capacity is not enough for the current element, this exception is thrown</exception>
        /// <param name="newCapacity"></param>
        public void SetCapacity(int newCapacity)
        {
            if (newCapacity < storedObjects.Count )
                throw new System.ArgumentException("The capacity provided is not enough for the current list");

            storedObjects.Capacity = newCapacity;
        }

        /// <summary>
        /// Creates and stores an amount of objects that are identical to the sample/template.
        /// If the created objects exceed the capacity, the pool automatically resizes.
        /// </summary>
        /// <param name="count"></param>
        public void StoreObjects(int count)
        {
            for(int i = 0; i < count; i++)
            {
                //Create a clone and store to the collection
                Type newObj = instantiationProvider.Instantiate(DefaultObject);
                StoreObject(newObj);
            }
        } 

        /// <summary>
        /// Stores a created object into the collection
        /// </summary>
        /// <param name="obj"></param>
        private void StoreObject(Type obj) {
            storedObjects.Add(obj);
        }

        /// <summary>
        /// Returns an available object from the pool.
        /// If no object is available, it crates a new one.
        /// </summary>
        public Type FindObject()
        {
            //If the storage is at the moment empty
            if(storedObjects.Count == 0)
            {
                //Store 1 object.
                StoreObjects(1);
            }

            Type obj = storedObjects[0];

            //Clear child
            storedObjects.RemoveAt(0);

            //Return the first object from the collection.
            return obj;
        }

        /// <summary>
        /// Clrears the whole storage and empties the pool.
        /// </summary>
        public void ClearStorage()
        {
            storedObjects.Clear();
        }
    }
}