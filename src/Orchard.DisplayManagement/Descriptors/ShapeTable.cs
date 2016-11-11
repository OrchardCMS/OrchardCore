using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors
{
    public class ShapeTable
    {
        public IDictionary<string, ShapeDescriptor> Descriptors { get; set; }
        public IDictionary<string, ShapeBinding> ShapeBindings { get; set; }

        public IDictionary<string, ShapeBinding> Bindings
        {
            get { return new TableBindings(this); }

            set { ShapeBindings = value; }
        }

        public class TableBindings : IDictionary<string, ShapeBinding>
        {
            private ShapeTable _shapeTable;

            public TableBindings(ShapeTable shapeTable)
            {
                _shapeTable = shapeTable;
            }

            public ShapeBinding this[string shapeAlternate]
            {
                get
                {
                    ShapeBinding shapeBinding;
                    TryGetValue(shapeAlternate, out shapeBinding);
                    return shapeBinding;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public int Count {
                get
                {
                    return _shapeTable.ShapeBindings.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public ICollection<string> Keys
            {
                get
                {
                    return _shapeTable.ShapeBindings.Keys;
                }
            }

            public ICollection<ShapeBinding> Values { get { throw new NotImplementedException(); } }
            public void Add(KeyValuePair<string, ShapeBinding> item) { throw new NotImplementedException(); }
            public void Add(string key, ShapeBinding value) { throw new NotImplementedException(); }
            public void Clear() { throw new NotImplementedException(); }
            public bool Contains(KeyValuePair<string, ShapeBinding> item) { throw new NotImplementedException(); }

            public bool ContainsKey(string shapeAlternate)
            {
                return _shapeTable.ShapeBindings.ContainsKey(shapeAlternate);
            }

            public void CopyTo(KeyValuePair<string, ShapeBinding>[] array, int arrayIndex) { throw new NotImplementedException(); }
            public IEnumerator<KeyValuePair<string, ShapeBinding>> GetEnumerator() { throw new NotImplementedException(); }
            public bool Remove(KeyValuePair<string, ShapeBinding> item) { throw new NotImplementedException(); }
            public bool Remove(string key) { throw new NotImplementedException(); }

            public bool TryGetValue(string shapeAlternate, out ShapeBinding binding)
            {
                ShapeBinding shapeBinding;
                if (_shapeTable.ShapeBindings.TryGetValue(shapeAlternate, out shapeBinding))
                {
                    var index = shapeAlternate.IndexOf("__");
                    var shapeType = index < 0 ? shapeAlternate : shapeAlternate.Substring(0, index);

                    ShapeDescriptor descriptor;
                    if (_shapeTable.Descriptors.TryGetValue(shapeType, out descriptor))
                    {
                        binding = new ShapeBinding
                        {
                            ShapeDescriptor = descriptor,
                            BindingName = shapeBinding.BindingName,
                            BindingSource = shapeBinding.BindingSource,
                            BindingAsync = shapeBinding.BindingAsync
                        };

                        return true;
                    }
                }

                binding = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
        }
    }
}