using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.Json;
using System.Threading.Tasks;

namespace aspapitest.Controllers
{

    public class JsonObject : Dictionary<string, object>
    {
        public string AsString(string key)
        {
            object o;
            if (TryGetValue(key, out o) == false) return string.Empty;
            return o.ToString();
        }

        public int AsInterger(string key)
        {
            object o;
            if (TryGetValue(key, out o) == false) return 0;
            if(o is JsonElement) {
                return ((JsonElement)o).GetInt32();
            }else if(o is int) {
                return (int)o;
            } else {
                return int.Parse(o.ToString());
            }
        }
    }

    [ApiController]
    public class TestController : ControllerBase
    {
        private static SerialPort sp = null;

        private void Open(JsonObject v)
        {
            string port = v.AsString("port");
            if (string.IsNullOrEmpty(port)) {
#if DEBUG
                port = "COM3";
#else
                var pns = SerialPort.GetPortNames();
                if (pns.Length == 0) return false;
                port = pns[0];
#endif
            }

            int brate = v.AsInterger("baudrate");
            if (brate == 0) brate = 115200;

            Close();
            sp = new SerialPort(port, brate);
            sp.Open();
        }

        private bool Close()
        {
            if (sp != null) {
                sp.Close();
                sp.Dispose();
                sp = null;
            }
            return true;
        }

        private bool Write(JsonObject v)
        {
            if (sp == null) return false;

            string s = v.AsString("value");
            try {
                if (s.EndsWith("\n") == false) s += "\n";
                sp.Write(s);
                return true;
            } catch {
                return false;
            }
        }

        private Task<string> Read()
        {
            return Task.Run(() => {
                string s = sp.ReadTo("\n");
                s = s.Trim('\r', '\n');
                return s;
            });
        }

        [HttpPost]
        [Route("serial/")]
        public async Task<JsonResult> Post(JsonObject v)
        {
            var r = new JsonObject();

            string action = v.AsString("action");
            r["action"] = action;
            r["error"] = false;
            r["value"] = string.Empty;

            try {
                switch (action) {
                case "open":
                    Open(v);
                    break;
                case "close":
                    Close();
                    break;
                case "read":
                    r["value"] = await Read();
                    break;
                case "write":
                    Write(v);
                    break;
                }
            } catch(Exception ex) {
                r["error"] = true;
                r["value"] = ex.Message;
            }

            return new JsonResult(r);
        }
    }
}
