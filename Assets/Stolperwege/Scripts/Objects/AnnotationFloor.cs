using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationFloor : InteractiveObject
{

    public override void Awake()
    {
        base.Awake();

        OnLongClick += () => { ShowBuildingList(StolperwegeHelper.PointerSphere.transform.position); };
    }

    private void ShowBuildingList(Vector3 position)
    {
        StartCoroutine(SceneController.GetInterface<StolperwegeInterface>().GetAllElements("UnityBuilding", false, (HashSet<StolperwegeElement> result) => {

            CircleMenu menu = (((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")))).GetComponent<CircleMenu>();

            Hashtable types = new Hashtable();
            foreach (StolperwegeElement element in result)
            {
                if(element.Value.Length > 0 && !types.Contains(element.Value))
                    types.Add(element.Value, element);
            }

            menu.Init((string key, object o) =>
            {
                UnityBuilding building = (UnityBuilding)o;

                building.Draw();

                Bounds b = StolperwegeHelper.GetBoundsOfChilds(building.StolperwegeObject.gameObject);

                building.StolperwegeObject.transform.position = position + Vector3.up * -position.y;

            }, types);

            StolperwegeHelper.PlaceInFrontOfUser(menu.transform, 0.5f);
            menu.transform.localScale = Vector3.one * 0.2f;

        }));

        
    }
}
