using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Сonsumer
{
    public class ContinuationSync<T> : IDisposable
    {

        const string PATH = "States";
        readonly string _name;

        public ContinuationSync(string name)
        {
            if(string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            if (!Directory.Exists(PATH))
                Directory.CreateDirectory(PATH);

            _name = Path.Combine(PATH, name);
        }

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
