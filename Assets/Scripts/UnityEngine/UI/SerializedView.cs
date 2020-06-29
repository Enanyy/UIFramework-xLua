using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public enum SerializedType
    {
        None = -1,
        Boolean = 0,
        Integer = 1,
        Float = 2,
        Enum = 3,
        String = 4,
        Object = 5,
        Vector2 = 6,
        Vector3 = 7,
        Vector4 = 8,
    }


    [AttributeUsage(AttributeTargets.Enum)]
    public class SerializedEnumAttribute : Attribute
    {
    }
    [Serializable]
    public class SerializedField
    {
        public string name;
        public SerializedType type = SerializedType.None;

        /// <summary>
        /// 相当于联合体
        /// </summary>
        public float[] values;
        public string stringValue;
        public UnityEngine.Object objectValue;

        public void SetInt(int value)
        {
            if (values == null || values.Length != 1)
            {
                values = new float[1];
            }
            values[0] = value;
            objectValue = null;
            stringValue = null;

            type = SerializedType.Integer;
        }
        public int GetInt(int defaultValue = 0)
        {
            if (type == SerializedType.Integer)
            {
                if (values != null && values.Length == 1)
                {
                    return (int)values[0];
                }
            }
            return defaultValue;
        }
        public void SetFloat(float value)
        {
            if (values == null || values.Length != 1)
            {
                values = new float[1];
            }
            values[0] = value;

            objectValue = null;
            stringValue = null;

            type = SerializedType.Float;
        }

        public float GetFloat(float defaultValue = 0)
        {
            if (type == SerializedType.Float)
            {
                if (values != null && values.Length == 1)
                {
                    return values[0];
                }
            }
            return defaultValue;
        }

        public void SetBool(bool value)
        {
            if (values == null || values.Length != 1)
            {
                values = new float[1];
            }
            values[0] = value ? 1 : -1;

            objectValue = null;
            stringValue = null;

            type = SerializedType.Boolean;
        }
        public bool GetBool(bool defaultValue = false)
        {
            if (type == SerializedType.Boolean)
            {
                if (values != null && values.Length == 1)
                {
                    return values[0] > 0;
                }
            }
            return defaultValue;
        }
        public void SetString(string value)
        {
            stringValue = value;

            values = null;
            objectValue = null;

            type = SerializedType.String;
        }
        public string GetString(string defaultValue = "")
        {
            if (type == SerializedType.String)
            {
                return stringValue;
            }
            return defaultValue;
        }

        public void SetVector2(Vector2 value)
        {
            if (values == null || values.Length != 2)
            {
                values = new float[2];
            }
            values[0] = value.x;
            values[1] = value.y;

            objectValue = null;
            stringValue = null;

            type = SerializedType.Vector2;

        }
        public Vector2 GetVector2()
        {
            if (type == SerializedType.Vector2)
            {
                if (values != null && values.Length == 2)
                {
                    return new Vector2(values[0], values[1]);
                }
            }
            return Vector2.zero;

        }
        public void SetVector3(Vector3 value)
        {
            if (values == null || values.Length != 3)
            {
                values = new float[3];
            }
            values[0] = value.x;
            values[1] = value.y;
            values[2] = value.z;

            objectValue = null;
            stringValue = null;

            type = SerializedType.Vector3;
        }
        public Vector3 GetVector3()
        {
            if (type == SerializedType.Vector3)
            {
                if (values != null && values.Length == 3)
                {
                    return new Vector3(values[0], values[1], values[2]);
                }
            }
            return Vector3.zero;

        }

        public void SetVector4(Vector4 value)
        {
            if (values == null || values.Length != 4)
            {
                values = new float[4];
            }
            values[0] = value.x;
            values[1] = value.y;
            values[2] = value.z;
            values[3] = value.w;

            objectValue = null;
            stringValue = null;

            type = SerializedType.Vector4;
        }
        public Vector4 GetVector4()
        {
            if (type == SerializedType.Vector4)
            {
                if (values != null && values.Length == 4)
                {
                    return new Vector4(values[0], values[1], values[2], values[3]);
                }
            }
            return Vector4.zero;

        }


        public void SetEnum(int value)
        {
            if (values == null || values.Length != 1)
            {
                values = new float[1];
            }
            values[0] = value;

            objectValue = null;

            type = SerializedType.Enum;
        }
        public int GetEnum(int defaultValue = 0)
        {
            if (type == SerializedType.Enum)
            {
                if (values != null && values.Length == 1)
                {
                    return (int)values[0];
                }
            }
            return defaultValue;
        }
        public string GetTypeName()
        {
            return stringValue;
        }

        public void SetTypeName(SerializedType type, string typeName)
        {
            this.type = type;
            this.stringValue = typeName;
        }
        public void SetObject(UnityEngine.Object obj)
        {
            objectValue = obj;
            if (obj != null)
            {
                stringValue = obj.GetType().FullName;
            }

            values = null;

            this.type = SerializedType.Object;
        }
        public UnityEngine.Object GetObject()
        {
            if (type == SerializedType.Object)
            {
                return objectValue;
            }
            return null;
        }
    }
    public class SerializedView : MonoBehaviour
    {
        [HideInInspector]
        public List<SerializedField> fields = new List<SerializedField>();

        private Dictionary<string, int> name2IndexDic = new Dictionary<string, int>();

        public SerializedField this[string name]
        {
            get
            {
                if (name2IndexDic.Count == 0)
                {
                    for (int i = 0; i < fields.Count; ++i)
                    {
                        name2IndexDic.Add(fields[i].name, i);
                    }
                }
                name2IndexDic.TryGetValue(name, out int index);
                if (index >= 0)
                {
                    return fields[index];
                }
                return null;
            }
        }
        public bool GetBoolField(string name, bool defaultValue = false)
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetBool(defaultValue);
            }
            return defaultValue;
        }

        public int GetIntField(int defaultValue = 0)
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetInt(defaultValue);
            }
            return defaultValue;
        }
        public float GetFloadField(float defaultValue = 0)
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetFloat(defaultValue);
            }
            return defaultValue;
        }

        public string GetStringField(string defaultValue = "")
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetString();
            }
            return defaultValue;
        }

        public int GetEnumField(string name, int defaultValue = 0)
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetEnum();
            }
            return defaultValue;
        }

        public Vector2 GetVector2Field(string name, Vector2 defaultValue)
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetVector2();
            }
            return defaultValue;
        }

        public Vector3 GetVector3Field(string name, Vector3 defaultValue)
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetVector3();
            }
            return defaultValue;
        }
        public Vector4 GetVector4Field(string name, Vector4 defaultValue)
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetVector4();
            }
            return defaultValue;
        }

        public Component GetComponentField(string name)
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetObject() as Component;
            }
            return null;
        }

        public T GetComponentField<T>(string name) where T : Component
        {
            var field = this[name];
            if (field != null)
            {
                T component = field.GetObject() as T;
                if (component != null)
                {
                    return component;
                }
                else
                {
                    GameObject go;
                    if (component != null)
                    {
                        go = component.gameObject;
                    }
                    else
                    {
                        go = GetGameObjectField(name);
                    }
                    if (go != null)
                    {
                        go.TryGetComponent(out component);

                        return component;
                    }
                }
            }
            return null;
        }

        public Component GetComponentField(string name, Type type)
        {
            var field = this[name];
            if (field != null)
            {
                Component component = field.GetObject() as Component;
                if (component != null && component.GetType() == type)
                {
                    return component;
                }
                else
                {
                    GameObject go;
                    if (component != null)
                    {
                        go = component.gameObject;
                    }
                    else
                    {
                        go = GetGameObjectField(name);
                    }
                    if (go != null)
                    {
                        go.TryGetComponent(type, out component);

                        return component;
                    }
                }
            }
            return null;
        }

        public T GetObjectField<T>(string name) where T : UnityEngine.Object
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetObject() as T;
            }
            return null;
        }
        public UnityEngine.Object GetObjectField(string name)
        {
            var field = this[name];
            if (field != null)
            {
                return field.GetObject();
            }
            return null;
        }

        public GameObject GetGameObjectField(string name)
        {
            var field = this[name];
            if (field != null)
            {
                GameObject go = field.GetObject() as GameObject;
                if (go != null)
                {
                    return go;
                }
                else
                {
                    Component component = field.GetObject() as Component;
                    if (component != null)
                    {
                        return component.gameObject;
                    }
                }
            }
            return null;
        }




        public void AddClick(string name, UnityAction call)
        {
            Button button = GetComponentField<Button>(name);
            if (button != null)
            {
                button.onClick.AddListener(call);
            }
        }
        public void SetText(string name, string text)
        {
            Text button = GetComponentField<Text>(name);
            if (button != null)
            {
                button.text = text;
            }
        }

        public void SetActive(string name, bool active)
        {
            GameObject go = GetGameObjectField(name);
            if (go != null && go.activeSelf != active)
            {
                go.SetActive(active);
            }
        }
    }
}