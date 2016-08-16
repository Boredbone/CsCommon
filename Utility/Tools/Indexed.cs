using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Boredbone.Utility.Tools
{
    public class Indexed<T> : INotifyPropertyChanged
    {
        public int Index
        {
            get { return _fieldIndex; }
            set
            {
                if (_fieldIndex != value)
                {
                    _fieldIndex = value;
                    RaisePropertyChanged(nameof(Index));
                }
            }
        }
        private int _fieldIndex;

        public T Value
        {
            get { return _fieldValue; }
            set
            {
                if (_fieldValue == null && value == null)
                {
                    return;
                }
                if (_fieldValue == null || !_fieldValue.Equals(value))
                {
                    _fieldValue = value;
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }
        private T _fieldValue;

        public Indexed()
        {
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
