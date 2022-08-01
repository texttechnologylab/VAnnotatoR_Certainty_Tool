using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UMAManagerInterface : Interface
{
    private GameObject ObjectContainer;
    private List<UMAISOObject> UMAISOObjects = new List<UMAISOObject>();
    private List<UMAISOEntity> UmaIsoEntities = new List<UMAISOEntity>();
    public AvatarObjectPanel AvatarObjectPanel { get; private set; }

    public void SetAvatarObjectPanel(InteractiveShapeNetObject iShapeNetObject)
    {
        if (AvatarObjectPanel == null)
        {
            AvatarObjectPanel = ((GameObject)Instantiate(Resources.Load("Prefabs/AvatarObjectPanel/AvatarObjectPanel"))).GetComponent<AvatarObjectPanel>();
            AvatarObjectPanel.Initialize(ObjectContainer, iShapeNetObject);
        }
        else
        {
            AvatarObjectPanel.SafeDestroy(() =>
            {
                if(AvatarObjectPanel.iShapeObject == iShapeNetObject) Destroy(AvatarObjectPanel.gameObject);
                else
                {
                    Destroy(AvatarObjectPanel.gameObject);
                    AvatarObjectPanel = ((GameObject)Instantiate(Resources.Load("Prefabs/AvatarObjectPanel/AvatarObjectPanel"))).GetComponent<AvatarObjectPanel>();
                    AvatarObjectPanel.Initialize(ObjectContainer, iShapeNetObject);
                }
            });
        }
        if (AvatarObjectPanel != null && AvatarObjectPanel.gameObject.activeInHierarchy) StolperwegeHelper.PlaceInFrontOfUser(AvatarObjectPanel.transform, 0.5f);
    }

    public void CloseActivePanel()
    {
        if(AvatarObjectPanel != null && AvatarObjectPanel.gameObject != null) Destroy(AvatarObjectPanel.gameObject);
    }

    public List<IsoMetaLink> AttributeEntityLinks = new List<IsoMetaLink>();

    protected override IEnumerator InitializeInternal()
    {
        Name = "UMAManager";
        AttributeEntityLinks = new List<IsoMetaLink>();
        UMAISOObjects = new List<UMAISOObject>();
        UmaIsoEntities = new List<UMAISOEntity>();
        ObjectContainer = GameObject.Find("ObjectContainer");
        yield break;
    }

    private IsoSpatialEntity _parent;
    private IsoSpatialEntity _child;
    private UMAISOEntity _parentUMA;
    private UMAISOEntity _childUMA;
    public void BuildAvatars()
    {
        UMAISOObjects = new List<UMAISOObject>();
        UmaIsoEntities = new List<UMAISOEntity>();
        if (AttributeEntityLinks == null || AttributeEntityLinks.Count == 0) return;

        foreach (IsoMetaLink link in AttributeEntityLinks)
        {
            _parent = (IsoSpatialEntity)link.Ground;
            _child = (IsoSpatialEntity)link.Figure;
            _parentUMA = UmaIsoEntities.Find(u => u.Entity.ID.Equals(_parent.ID));
            if (_parentUMA == null)
            {
                _parentUMA = new UMAISOEntity(_parent);
                UmaIsoEntities.Add(_parentUMA);
            }
            _childUMA = UmaIsoEntities.Find(u => u.Entity.ID.Equals(_child.ID));
            if (_childUMA == null)
            {
                _childUMA = new UMAISOEntity(_child);
                UmaIsoEntities.Add(_childUMA);
            }
            _parentUMA.AddChild(_childUMA);
        }
        foreach (UMAISOEntity entity in UmaIsoEntities)
        {
            if (entity.Name.Equals(UMAISOEntity.ROOT.Name)) CreateAvatar(entity);
        }
    }

    private GameObject _objectInstance;
    private void CreateAvatar(UMAISOEntity rootEntity)
    {
        _objectInstance = (GameObject)Instantiate(Resources.Load("Prefabs/Avatar/ShapeNetUMA"));
        _objectInstance.GetComponent<InteractiveShapeNetObject>().Init(rootEntity.Entity, true, true);

        _objectInstance.layer = 19;
        _objectInstance.transform.position = rootEntity.Entity.Position.Vector;
        _objectInstance.transform.rotation = rootEntity.Entity.Rotation.Quaternion;
        _objectInstance.transform.localScale = rootEntity.Entity.Scale.Vector;
        if(ObjectContainer == null) ObjectContainer = GameObject.Find("ObjectContainer");
        _objectInstance.transform.SetParent(ObjectContainer.transform, true);
        if (rootEntity.Properties?.Count != 0) StartCoroutine(_objectInstance.GetComponent<UMAISOObject>().Init(rootEntity, (obj) =>
        {
            UMAISOObjects.Add(obj);
        }));
    }
}