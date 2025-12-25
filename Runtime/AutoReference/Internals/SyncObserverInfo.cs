using System.Reflection;

namespace Teo.AutoReference.Internals {
    internal struct SyncObserverInfo {
        public FieldInfo targetField;
        public MethodInfo methodInfo;
    }
}
