using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class JsonSetting {
    private Dictionary<string, object> items = new Dictionary<string, object>();
    public JsonSetting Add(string key, object value) {
        items.Add(key, value);
        return this;
    }

    override
    public string ToString() {
        StringBuilder builder = new StringBuilder();
        builder.Append("{");
        int index = 0;
        foreach (KeyValuePair<string, object> item in items) {
            if (index > 0) builder.Append(",");
            builder.Append(item.Key);
            builder.Append(":");
            builder.Append(item.Value.ToString());
            index++;
        }
        builder.Append("}");
        return builder.ToString();
    }

    public JsonSetting loadJsonString(string str) {
        if (str.StartsWith("{") && str.EndsWith("}")) {
            str = str.Substring(1, str.Length - 2).Replace("\n", "");
            string[] options = str.Split(',');
            foreach (string item in options) {
                if (!item.Contains(":")) continue;
                items.Add(item.Split(':')[0].Trim(), item.Split(':')[1].Trim());
            }
        }
        return this;
    }

    public string Get(string key) {
        object value;
        if (items.TryGetValue(key, out value)) {
            return value.ToString();
        }
        return null;
    }
}
