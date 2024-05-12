using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


namespace k323.Commons.NetworkActionSystem {
    public static class ActionFactory {
        private static Dictionary<ActionID, ObjectPool<Action>> actionPools = new Dictionary<ActionID, ObjectPool<Action>>();
        
        private static ObjectPool<Action> GetActionPool(ActionID actionID) {
            // create new pool if there isn't one cached
            if (!actionPools.TryGetValue(actionID, out var actionPool)) {
                actionPool = new ObjectPool<Action>(
                    createFunc: () => Object.Instantiate(ActionPrototypeStore.GetActionPrototypeByID(actionID)),
                    actionOnRelease: action => action.Reset(),
                    actionOnDestroy: Object.Destroy);

                actionPools.Add(actionID, actionPool);
            }

            return actionPool;
        }

        /// <summary>
        /// Factory method that creates Actions from their request data.
        /// </summary>
        /// <param name="data">the data to instantiate this skill from. </param>
        /// <returns>the newly created action. </returns>
        public static Action CreateActionFromData(ref ActionPacket data) {
            var ret = GetActionPool(data.ActionID).Get();
            ret.Initialize(ref data);
            return ret;
        }


        public static void ReturnAction(Action action) {
            var pool = GetActionPool(action.ActionID);
            pool.Release(action);
        }
    }
}
    
