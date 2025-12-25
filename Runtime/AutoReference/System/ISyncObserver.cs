using UnityEngine;

namespace Teo.AutoReference.System {
    public interface ISyncObserver {
        public void OnSync(MonoBehaviour target);
    }
}
