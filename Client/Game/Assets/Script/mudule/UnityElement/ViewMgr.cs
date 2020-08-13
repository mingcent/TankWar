using UnityEngine;


public class ViewMgr
{
    public static void BindEntityView(BaseEntity entity)
    {
        var prefab = ResourseMgr.LoadPrefab(entity.PrefabName);
        if (null == prefab) return;
        var obj = (GameObject)GameObject.Instantiate(prefab,
                                                     entity.transform.position.vec3,
                                                     Quaternion.Euler(new Vector3(0, entity.transform.deg.i * 0.001f, 0)));
        var views = obj.GetComponents<BaseEntityView>();
        if (views.Length <= 0)
        {
            var view = obj.AddComponent<BaseEntityView>();
            view.BindEntity(entity);
        }
        else
        {
            foreach (var view in views)
            {
                view.BindEntity(entity);
            }
        }

    }

    public static void UnBindView(BaseEntity entity)
    {
        entity.view?.OnDead();
        entity.view = null;
    }
}

