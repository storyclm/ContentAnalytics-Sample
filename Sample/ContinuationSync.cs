using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Сonsumer
{
    public class ContinuationSync<T> : IDisposable
    {
        readonly string _name;

        public ContinuationSync(string name) =>
            _name = name ?? throw new ArgumentNullException(nameof(name));

        T _token { get; set; }

        public T Token
        {
            get
            {
                if (File.Exists(_name))
                    _token = JsonConvert.DeserializeObject<T>(File.ReadAllText(_name));

                return _token;
            }

            set
            {
                _token = value;
                Save();
            }
        }

        void Save() => File.WriteAllText(_name, JsonConvert.SerializeObject(_token), Encoding.UTF8);

        public void Dispose() => Save();
    }
}
