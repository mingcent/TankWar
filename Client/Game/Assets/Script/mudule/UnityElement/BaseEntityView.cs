using FixPoint;
using UnityEngine;


public class BaseEntityView : MonoBehaviour
{
    public const float LerpPercent = 0.3f;
    public BaseEntity entity;

    public virtual void BindEntity(BaseEntity e, BaseEntity oldEntity = null)
    {
        e.view = this;
        this.entity = e;
        var updateEntity = oldEntity ?? e;
        transform.position = updateEntity.transform.position.vec3;
        transform.rotation = Quaternion.Euler(0, updateEntity.transform.deg.i * 0.001f, 0);
    }


    public virtual void OnDead()
    {
        GameObject.Destroy(gameObject);
    }

    public void Update()
    {
        if (entity != null)
        {
            var pos = entity.transform.position.vec3;
            transform.position = Vector3.Lerp(transform.position, pos, Time.smoothDeltaTime/(World.UpdateInterval*0.001f));
            var deg = entity.transform.deg.i * 0.001f;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, deg, 0), Time.smoothDeltaTime / (World.UpdateInterval * 0.001f));
        }
    }

    public void ForceUpdate()
    { 
        if (entity != null)
        {
            var pos = entity.transform.position.vec3;
            transform.position = Vector3.Lerp(transform.position, pos, 1);
            var deg = entity.transform.deg.i * 0.001f;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, deg, 0), 1);
        }
    }

}
