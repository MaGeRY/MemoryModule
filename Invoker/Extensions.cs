using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace System
{
    public static class InvokerExtension
    {        
        public static T ToType<T>(this object value)
        {
            if (value is T)
            {
                return (T)value;
            }
            return default(T);
        }

        public static bool HasParameters(this MethodInfo method, params Type[] parameters)
        {
            ParameterInfo[] methodParams = method.GetParameters();
            if (parameters.Length != methodParams.Length) return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                // Allow when parameter is nulled //
                if (parameters[i] == null) continue;
                // Get parameter type of method parameter type //
                Type parameterType = methodParams[i].ParameterType;
                // Structs of parameters not equal //
                if (parameters[i].IsByRef != parameterType.IsByRef)
                {
                    return false;
                }
                // Get type of element when parameter is array //
                if (parameterType.IsByRef)
                {
                    parameters[i] = parameters[i].GetElementType();
                    parameterType = parameterType.GetElementType();                    
                }
                // Allow when parameter is object //
                if (parameters[i] == typeof(object)) continue;
                // Allow when parameter equaled of method type //
                if (parameters[i] == parameterType) continue;
                // Try find type from base type //
                while (parameters[i] != null && parameters[i] != parameterType)
                {
                    parameterType = parameterType.BaseType;
                }
                // Allow when types is equaled //
                if (parameters[i] == parameterType) continue;
                return false;
            }

            return true;
        }        

        public static FieldInfo FindField(this Type type, string name)
        {
            if (type != null)
            {
                foreach (FieldInfo fieldInfo in type.GetFields(Invoker.AllBindingFlags))
                {
                    if (fieldInfo.Name == name) return fieldInfo;
                }
            }
            return null;
        }

        public static MethodInfo FindMethod(this Type type, string name, params Type[] parameters)
        {
            if (type != null)
            {
                foreach (MethodInfo methodInfo in type.GetMethods(Invoker.AllBindingFlags))
                {
                    if (methodInfo.Name == name)
                    {
                        if (parameters.Length == 0 || methodInfo.HasParameters(parameters))
                        {
                            return methodInfo;
                        }
                    }
                }
            }
            return null;
        }

        public static object Invoke(this MethodInfo method, object target, params object[] parameters)
        {
            if (method == null) return null;
            return method.Invoke(target, parameters);
        }

        public static T GetValue<T>(this object target, string name, object[] index = null)
        {
            object result = Invoker.GetValue(name, target, index);
            if (result is T)
            {
                return (T)result;
            }
            return default(T);
        }

        public static bool GetValue<T1,T2>(this T1 target, string name, out T2 value, object[] index = null)
        {
            object result = Invoker.GetValue(name, target, index);
            if (result is T2)
            {
                value = (T2)result;
                return true;
            }
            value = default(T2);
            return false;
        }

        public static T2 SetValue<T1,T2>(this T1 target, string name, T2 value)
        {
            return Invoker.SetValue(target, name, value, null);
        }

        public static T2 SetValue<T1,T2>(this T1 target, string name, object[] index, T2 value)
        {
            return Invoker.SetValue(target, name, value, index);
        }

        public static object Invoke<T>(this T target, string method, object[] parameters)
        {
            return Invoker.Call(method, target, parameters);
        }

        public static object InvokeBase<T>(this T target, string method, object[] parameters)
        {
            return Invoker.CallBase(method, target, parameters);
        }

        public static object Call<T>(this T target, string method, params object[] parameters)
        {
            return Invoker.Call(method, target, parameters);
        }

        public static object CallBase<T>(this T target, string method, params object[] parameters)
        {
            return Invoker.CallBase(method, target, parameters);
        }
    }
}
