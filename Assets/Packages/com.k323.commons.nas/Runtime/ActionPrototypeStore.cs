using System.Collections.Generic;
using UnityEngine;


namespace k323.Commons.NetworkActionSystem {
    public class ActionPrototypeStore : MonoBehaviour {
        [Tooltip("Place here all action prototypes that you want to be able to use in the game.")]
        [SerializeField] private List<NetworkAction> actionPrototypes;

        private static ActionPrototypeStore main;
        private List<NetworkAction> allActions;
        
        protected virtual void Awake() {
            if (main != null) {
                Destroy(gameObject);
                return;
            }
            main = this;

            InitializeStore();
            DontDestroyOnLoad(gameObject);
        }

		protected virtual void OnDestroy() {
			if (main == this) {
				main = null;
			}
		}

        protected virtual void InitializeStore() {
            var uniqueActions = new HashSet<NetworkAction>(actionPrototypes);
            allActions = new List<NetworkAction>(uniqueActions.Count);

            int i = 0;
            foreach (var uniqueAction in uniqueActions) {
                uniqueAction.ActionID = new ActionID { ID = i };
                allActions.Add(uniqueAction);
                i++;
            }
        }

        public static NetworkAction GetActionPrototypeByID(ActionID index) {
            if (main == null) {
                Debug.LogError("ActionPrototypeStore is not initialized.");
                return null;
            }
            return main.allActions[index.ID];
        }
    }
}
