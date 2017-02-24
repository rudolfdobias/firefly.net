using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;

namespace Firefly.Helpers
{
    public class QueryString
    {
        private readonly Dictionary<string, string> _store;

        public QueryString(Dictionary<string, string> values)
        {
            _store = values;
        }

        public QueryString()
        {
            _store = new Dictionary<string, string>();
        }

        public QueryString Add(string key, string val)
        {
            _store.Add(key, val);
            return this;
        }

        public QueryString Replace(string key, string val)
        {
            _store[key] = val;
            return this;
        }

        public QueryString Remove(string key)
        {
            _store.Remove(key);
            return this;
        }

        public override string ToString()
        {
            var array = (from pair in _store
                select string.Format("{0}={1}", HtmlEncoder.Default.Encode(pair.Key),
                    HtmlEncoder.Default.Encode(pair.Value)));
            return "?" + string.Join("&", array);
        }
    }
}