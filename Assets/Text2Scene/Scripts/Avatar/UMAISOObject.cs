using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UMAISOObject : MonoBehaviour
{
    // Root Node
    public UMAISOEntity Root;

    // Children of Root Node
    public UMAISOEntity Head;
    public UMAISOEntity Torso;
    public UMAISOEntity Legs;

    // Children of Torso Node
    public UMAISOEntity Arms;

    // Children of Head Node
    public UMAISOEntity Ears;
    public UMAISOEntity Face;

    // Children of Face Node
    public UMAISOEntity Eyes;
    public UMAISOEntity Nose;
    public UMAISOEntity Mouth;

    // Wardrobe Nodes
    public UMAISOEntity Wardrobe;

    public bool Initialized = false;

    private List<UMAISOEntity> Nodes = new List<UMAISOEntity>();
    private List<Dictionary<string, object>> SpatialEntities = new List<Dictionary<string, object>>();
    private List<Dictionary<string, object>> Links = new List<Dictionary<string, object>>();
    private List<IsoSpatialEntity> entities = new List<IsoSpatialEntity>();
    private Dictionary<UMAISOEntity, List<Dictionary<string, object>>> ObjectFeatures = new Dictionary<UMAISOEntity, List<Dictionary<string, object>>>();

    private UMAController AvatarController;

    private TextAnnotatorInterface TextAnnotator
    {
        get { return SceneController.GetInterface<TextAnnotatorInterface>(); }
    }

    private UMAManagerInterface UMAManager
    {
        get { return SceneController.GetInterface<UMAManagerInterface>(); }
    }
    
    public void Awake()
    {
        AvatarController = GetComponentInChildren<UMAController>();
        Root = new UMAISOEntity(UMAISOEntity.ROOT.Name);

        Head = new UMAISOEntity(UMAISOEntity.HEAD.Name, Root);
        Torso = new UMAISOEntity(UMAISOEntity.TORSO.Name, Root);
        Legs = new UMAISOEntity(UMAISOEntity.LEGS.Name, Root);
        Wardrobe = new UMAISOEntity(UMAISOEntity.WARDROBE.Name, Root);

        Arms = new UMAISOEntity(UMAISOEntity.ARMS.Name, Torso);

        Ears = new UMAISOEntity(UMAISOEntity.EARS.Name, Head);
        Face = new UMAISOEntity(UMAISOEntity.FACE.Name, Head);

        Eyes = new UMAISOEntity(UMAISOEntity.EYES.Name, Face);
        Nose = new UMAISOEntity(UMAISOEntity.NOSE.Name, Face);
        Mouth = new UMAISOEntity(UMAISOEntity.MOUTH.Name, Face);

        Queue<UMAISOEntity> EntityQueue = new Queue<UMAISOEntity>();
        EntityQueue.Enqueue(Root);
        while(EntityQueue.Count > 0)
        {
            UMAISOEntity qued = EntityQueue.Dequeue();
            Nodes.Add(qued);
            if (qued.Children?.Count > 0)
            {
                foreach (UMAISOEntity child in qued.Children)
                {
                    EntityQueue.Enqueue(child);
                }
            }
        }
    }

    public delegate void OnInitializedNew(IsoSpatialEntity root);
    public IEnumerator Init(UMAController avatar, OnInitializedNew onInit)
    {
        while (!AvatarController.Initialized) yield return null;
        _init(avatar, onInit, Vector3.zero, Quaternion.identity, Vector3.one);
    }


    public IEnumerator Init(UMAController avatar, OnInitializedNew onInit, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        while (!AvatarController.Initialized) yield return null;
        _init(avatar, onInit, pos, rot, scale);
    }

    private IsoSpatialEntity entity;
    private void _init(UMAController avatar, OnInitializedNew onInit, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        InitObjectFeatures();
        foreach (UMAISOEntity umaIsoEntity in ObjectFeatures.Keys)
        {
            SpatialEntities.Add(TextAnnotator.CreateSpatialEntityAttributeMap(umaIsoEntity.Name, pos, rot, scale, featureMap: ObjectFeatures[umaIsoEntity]));
        }
        entities = new List<IsoSpatialEntity>();
        TextAnnotator.OnElemsCreated = (created) =>
        {
            TextAnnotator.OnElemsCreated = null;
            foreach (AnnotationBase item in created)
            {
                if(item is IsoSpatialEntity)
                {
                    entity = (IsoSpatialEntity)item;
                    UMAISOEntity umaisoEntity = Nodes.Find(e => entity.Object_ID.Equals(e.Name));
                    umaisoEntity.Entity = entity;
                    if (umaisoEntity.Name.Equals(UMAISOEntity.ROOT.Name)) onInit(umaisoEntity.Entity);
                }
            }
            foreach (UMAISOEntity node in Nodes)
            {
                if (node.Parent != null) Links.Add(TextAnnotator.CreateLinkAttributeMap(node.Entity, node.Parent.Entity, "PartCoreference"));
            }
            TextAnnotator.FireWorkBatchCommand(null, new Dictionary<string, List<Dictionary<string, object>>> { { AnnotationTypes.META_LINK, Links } }, null, null);
            Initialized = true;
        };
        TextAnnotator.FireWorkBatchCommand(null, new Dictionary<string, List<Dictionary<string, object>>> { { AnnotationTypes.SPATIAL_ENTITY, SpatialEntities } }, null, null);
    }

    public delegate void OnInitialized(UMAISOObject obj);
    public IEnumerator Init(UMAISOEntity root, OnInitialized onInit)
    {
        while (!GetComponentInChildren<UMAController>().Initialized) yield return false;
        Root = root;
        Head = Root.Children.Find(u => u.Name.Equals(UMAISOEntity.HEAD.Name));
        Torso = Root.Children.Find(u => u.Name.Equals(UMAISOEntity.TORSO.Name));
        Legs = Root.Children.Find(u => u.Name.Equals(UMAISOEntity.LEGS.Name));
        Wardrobe = Root.Children.Find(u => u.Name.Equals(UMAISOEntity.WARDROBE.Name));
        Ears = Head.Children.Find(u => u.Name.Equals(UMAISOEntity.EARS.Name));
        Face = Head.Children.Find(u => u.Name.Equals(UMAISOEntity.FACE.Name));
        Arms = Torso.Children.Find(u => u.Name.Equals(UMAISOEntity.ARMS.Name));
        Eyes = Face.Children.Find(u => u.Name.Equals(UMAISOEntity.EYES.Name));
        Nose = Face.Children.Find(u => u.Name.Equals(UMAISOEntity.NOSE.Name));
        Mouth = Face.Children.Find(u => u.Name.Equals(UMAISOEntity.MOUTH.Name));
        GetComponentInChildren<UMAController>().BuildAvatar(Root.Properties[UMAProperty.MODEL], Root.Properties[UMAProperty.SKIN_COLOR], Root.GetChildProperties());
        onInit(this);
    }

    private Dictionary<string, Dictionary<string, object>> _changes;
    public Dictionary<string, Dictionary<string, object>> GetChanges()
    {
        _changes = new Dictionary<string, Dictionary<string, object>>();
        Root.GetChanges(_changes);
        return _changes;
    }

    private void InitObjectFeatures()
    {
        ObjectFeatures = new Dictionary<UMAISOEntity, List<Dictionary<string, object>>>
        {
            {
                UMAISOEntity.ROOT,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.SKIN_COLOR.Value, "#" + ColorUtility.ToHtmlStringRGB(AvatarController.GetSkinColor()) } },
            new Dictionary<string, object> {{ UMAProperty.MODEL.Value, AvatarController.GetModel().Value }},
            new Dictionary<string, object> {{ UMAProperty.HEIGHT.Value, AvatarController.GetValue(UMAProperty.HEIGHT) } },
            new Dictionary<string, object> {{ UMAProperty.LOWER_MUSCLE.Value, AvatarController.GetValue(UMAProperty.LOWER_MUSCLE) }},
            new Dictionary<string, object> {{ UMAProperty.LOWER_WEIGHT.Value, AvatarController.GetValue(UMAProperty.LOWER_WEIGHT) }},
            new Dictionary<string, object> {{ UMAProperty.SKIN_BLUENESS.Value, AvatarController.GetValue(UMAProperty.SKIN_BLUENESS) }},
            new Dictionary<string, object> {{ UMAProperty.SKIN_GREENNESS.Value, AvatarController.GetValue(UMAProperty.SKIN_GREENNESS) }},
            new Dictionary<string, object> {{ UMAProperty.SKIN_REDNESS.Value, AvatarController.GetValue(UMAProperty.SKIN_REDNESS) }},
            new Dictionary<string, object> {{ UMAProperty.UPPER_MUSCLE.Value, AvatarController.GetValue(UMAProperty.UPPER_MUSCLE) }},
            new Dictionary<string, object> {{ UMAProperty.UPPER_WEIGHT.Value, AvatarController.GetValue(UMAProperty.UPPER_WEIGHT) }},
        }
            },

            {
                UMAISOEntity.HEAD,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.FOREHEAD_POSTION.Value, AvatarController.GetValue(UMAProperty.FOREHEAD_POSTION) }},
            new Dictionary<string, object> {{ UMAProperty.FOREHEAD_SIZE.Value, AvatarController.GetValue(UMAProperty.FOREHEAD_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.HEAD_SIZE.Value, AvatarController.GetValue(UMAProperty.HEAD_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.HEAD_WIDTH.Value, AvatarController.GetValue(UMAProperty.HEAD_WIDTH) }},
            new Dictionary<string, object> {{ UMAProperty.NECK_THICKNESS.Value, AvatarController.GetValue(UMAProperty.NECK_THICKNESS) }},
        }
            },

            {
                UMAISOEntity.FACE,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.CHEEK_POSITION.Value, AvatarController.GetValue(UMAProperty.CHEEK_POSITION) }},
            new Dictionary<string, object> {{ UMAProperty.CHEEK_SIZE.Value, AvatarController.GetValue(UMAProperty.CHEEK_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.LOW_CHEEK_POSITION.Value, AvatarController.GetValue(UMAProperty.LOW_CHEEK_POSITION) }},
            new Dictionary<string, object> {{ UMAProperty.LOW_CHEEK_PRONOUNCED.Value, AvatarController.GetValue(UMAProperty.LOW_CHEEK_PRONOUNCED) }},
        }
            },

            {
                UMAISOEntity.ARMS,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.ARM_LENGTH.Value, AvatarController.GetValue(UMAProperty.ARM_LENGTH) }},
            new Dictionary<string, object> {{ UMAProperty.ARM_WIDTH.Value, AvatarController.GetValue(UMAProperty.ARM_WIDTH) }},
            new Dictionary<string, object> {{ UMAProperty.FOREARM_LENGTH.Value, AvatarController.GetValue(UMAProperty.FOREARM_LENGTH) }},
            new Dictionary<string, object> {{ UMAProperty.FOREARM_POSTION.Value, AvatarController.GetValue(UMAProperty.FOREARM_POSTION) }},
            new Dictionary<string, object> {{ UMAProperty.HAND_SIZE.Value, AvatarController.GetValue(UMAProperty.HAND_SIZE) }},
        }
            },

            {
                UMAISOEntity.LEGS,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.FEET_SIZE.Value, AvatarController.GetValue(UMAProperty.FEET_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.LEG_SEPERATION.Value, AvatarController.GetValue(UMAProperty.LEG_SEPERATION) }},
            new Dictionary<string, object> {{ UMAProperty.LEGS_SIZE.Value, AvatarController.GetValue(UMAProperty.LEGS_SIZE) }},
        }
            },

            {
                UMAISOEntity.MOUTH,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.CHIN_POSITION.Value, AvatarController.GetValue(UMAProperty.CHIN_POSITION) }},
            new Dictionary<string, object> {{ UMAProperty.CHIN_PRONOUNCED.Value, AvatarController.GetValue(UMAProperty.CHIN_PRONOUNCED) }},
            new Dictionary<string, object> {{ UMAProperty.CHIN_SIZE.Value, AvatarController.GetValue(UMAProperty.CHIN_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.JAWS_POSITION.Value, AvatarController.GetValue(UMAProperty.JAWS_POSITION) }},
            new Dictionary<string, object> {{ UMAProperty.JAWS_SIZE.Value, AvatarController.GetValue(UMAProperty.JAWS_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.LIPS_SIZE.Value, AvatarController.GetValue(UMAProperty.LIPS_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.MANDIBLE_SIZE.Value, AvatarController.GetValue(UMAProperty.MANDIBLE_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.MOUTH_SIZE.Value, AvatarController.GetValue(UMAProperty.MOUTH_SIZE) }},
        }
            },

            {
                UMAISOEntity.NOSE,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.NOSE_CURVE.Value, AvatarController.GetValue(UMAProperty.NOSE_CURVE) }},
            new Dictionary<string, object> {{ UMAProperty.NOSE_FLATTEN.Value, AvatarController.GetValue(UMAProperty.NOSE_FLATTEN) }},
            new Dictionary<string, object> {{ UMAProperty.NOSE_INCLANATION.Value, AvatarController.GetValue(UMAProperty.NOSE_INCLANATION) }},
            new Dictionary<string, object> {{ UMAProperty.NOSE_POSITION.Value, AvatarController.GetValue(UMAProperty.NOSE_POSITION) }},
            new Dictionary<string, object> {{ UMAProperty.NOSE_PRONOUNCED.Value, AvatarController.GetValue(UMAProperty.NOSE_PRONOUNCED) }},
            new Dictionary<string, object> {{ UMAProperty.NOSE_SIZE.Value, AvatarController.GetValue(UMAProperty.NOSE_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.NOSE_WIDTH.Value, AvatarController.GetValue(UMAProperty.NOSE_WIDTH) }},
        }
            },

            {
                UMAISOEntity.EARS,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.EARS_POSITION.Value, AvatarController.GetValue(UMAProperty.EARS_POSITION) }},
            new Dictionary<string, object> {{ UMAProperty.EARS_ROTATION.Value, AvatarController.GetValue(UMAProperty.EARS_ROTATION) }},
            new Dictionary<string, object> {{ UMAProperty.EARS_SIZE.Value, AvatarController.GetValue(UMAProperty.EARS_SIZE) }},
        }
            },

            {
                UMAISOEntity.EYES,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.EYE_ROTATION.Value, AvatarController.GetValue(UMAProperty.EYE_ROTATION) }},
            new Dictionary<string, object> {{ UMAProperty.EYE_SPACING.Value, AvatarController.GetValue(UMAProperty.EYE_SPACING) }},
            new Dictionary<string, object> {{ UMAProperty.EYE_SIZE.Value, AvatarController.GetValue(UMAProperty.EYE_SIZE) }},

        }
            },

            {
                UMAISOEntity.WARDROBE,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.WARDROBE_HAIR.Value, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_HAIR) }},
            new Dictionary<string, object> {{ UMAProperty.WARDROBE_CHEST.Value, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_CHEST) }},
            new Dictionary<string, object> {{ UMAProperty.WARDROBE_LEGS.Value, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_LEGS) }},
            new Dictionary<string, object> {{ UMAProperty.WARDROBE_UNDERWEAR.Value, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_UNDERWEAR) }},
            new Dictionary<string, object> {{ UMAProperty.WARDROBE_BEARD.Value, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_BEARD) }},
            new Dictionary<string, object> {{ UMAProperty.WARDROBE_EYEBROW.Value, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_EYEBROW) }},
            new Dictionary<string, object> {{ UMAProperty.WARDROBE_EYES.Value, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_EYES) }},
            new Dictionary<string, object> {{ UMAProperty.WARDROBE_FACE.Value, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_FACE) }},
            new Dictionary<string, object> {{ UMAProperty.WARDROBE_HANDS.Value, AvatarController.GetWardrobeValue(UMAProperty.WARDROBE_HANDS) }},
        }
            },

            {
                UMAISOEntity.TORSO,
                new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> {{ UMAProperty.BELLY.Value, AvatarController.GetValue(UMAProperty.BELLY) }},
            new Dictionary<string, object> {{ UMAProperty.BREAST_SIZE.Value, AvatarController.GetValue(UMAProperty.BREAST_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.GLUTEUS_SIZE.Value, AvatarController.GetValue(UMAProperty.GLUTEUS_SIZE) }},
            new Dictionary<string, object> {{ UMAProperty.WAIST.Value, AvatarController.GetValue(UMAProperty.WAIST) }},
        }
            }
        };
    }
}
