using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredicateObject : StolperwegeObject {

    public static StolperwegeInterface.RelationType mendatoryRole;
    public static StolperwegeInterface.RelationType optionalRole;

    public HashSet<StolperwegeElement> rolesMendatory;
    public HashSet<StolperwegeElement> rolesOptional;

    public Object rolePrefab;
    public Object propositionPrefab;

    public StolperwegeText RelatedText;
    public StolperwegeTermConnector termConnector;

    Transform Icon;

    public override StolperwegeElement Referent
    {
        get
        {
            return base.Referent;
        }

        set
        {
            base.Referent = value;
            InitRoles();
            Link();
            Label.Scale = -.35f;
        }
    }

    public override void Awake()
    {
        base.Awake();

        rolePrefab = Resources.Load("StolperwegeElements/ArgumentRoleObject");
        propositionPrefab = Resources.Load("StolperwegeElements/PropositionObject");

        Icon = transform.Find("Icon");
    }

    public override void Start()
    {
        base.Start();  
        LongClickTime = 1;
        OnLongClick = () => { ShowAddRolesMenu(); };
        Linkable = false;
    }

    public override void DeleteElement()
    {
        foreach (ArgumentRoleObject roleObject in GetComponentsInChildren<ArgumentRoleObject>())
        {
            DiscourseReferentObject dr = GetComponentInChildren<DiscourseReferentObject>();

            if (dr != null) dr.transform.parent = null;
        }

        base.DeleteElement();
    }

    private void Link()
    {
        
        StolperwegeInterface.RelationType termArgumentRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Term", "argument");
        StolperwegeInterface.RelationType termPredRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Term", "predicate");
        StolperwegeInterface.RelationType conTermRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.TermConnector", "terms");
        StolperwegeInterface.RelationType conPropRel = StolperwegeInterface.GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Proposition", "termconnectors");

        foreach (StolperwegeElement term in Referent.GetRelatedElementsByType(termPredRel))
        {
            /*
            foreach (StolperwegeElement text in term.GetRelatedElementsByType(termArgumentRel))
            {
                if (!(text is StolperwegeText) && RelatedText != null) continue;

                LinkToText((StolperwegeText)text);

                break;
            }*/

            foreach (StolperwegeElement termCon in term.GetRelatedElementsByType(conTermRel))
            {
                if (!(termCon is StolperwegeTermConnector)) continue;

                termConnector = (StolperwegeTermConnector)termCon;

                foreach(StolperwegeElement proposition in termConnector.GetRelatedElementsByType(conPropRel))
                {
                    if(proposition is Proposition && GetComponentInChildren<PropositionObject>() == null)
                    {
                        if (proposition.StolperwegeObject == null) proposition.Draw();
                        ((Predicate)Referent).proposition = (Proposition)proposition;
                        GameObject propObject = proposition.StolperwegeObject.gameObject;

                        propObject.transform.parent = transform;
                        propObject.transform.localPosition = Vector3.left * 1.25f;
                        break;
                    }
                }
                
                foreach(StolperwegeElement roleTerm in termConnector.GetRelatedElementsByType(conTermRel))
                {
                    if (roleTerm == term) continue;

                    foreach(StolperwegeElement role in roleTerm.GetRelatedElementsByType(termPredRel))
                    {
                        if (!(role is ArgumentRole)) continue;

                        foreach(ArgumentRoleObject roleObject in GetComponentsInChildren<ArgumentRoleObject>())
                            if(roleObject.Role == role)
                            {
                                roleObject.SetTerm((StolperwegeTerm)roleTerm);
                                break;
                            }
                        break;
                    }
                }
                break;
            }
            break;
        }
    }

    private void LinkToText(StolperwegeText text)
    {
        RelatedText = (StolperwegeText)text;

        StolperwegeRelation relation = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelation"))).GetComponent<StolperwegeRelation>();

        relation.StartAnchor.SetParent(transform, false);
        relation.EndAnchor.SetParent(RelatedText.StolperwegeObject.transform, false);
        relation.StartAnchor.localPosition = Vector3.forward * 0.1f;
        relation.EndAnchor.localPosition = Vector3.zero;
    }

    private void ShowAddRolesMenu()
    {
        CircleMenu menu = (((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")))).GetComponent<CircleMenu>();

        Hashtable types = new Hashtable();
        foreach (ArgumentRole role in ArgumentRole.Roles)
        {
            if(int.Parse((string)role.ID) <= 100 && (!(rolesMendatory.Contains(role)||rolesOptional.Contains(role))))
                types.Add(role.Value, role);
        }

        menu.Init((string key, object o) =>
        {
            ArgumentRole role = (ArgumentRole)o;

            this.AddRole(role,true);

            Referent.AddStolperwegeRelation(role, mendatoryRole, true);

        }, types);

        StolperwegeHelper.PlaceInFrontOfUser(menu.transform, 0.5f, false);
        menu.transform.forward = -StolperwegeHelper.CenterEyeAnchor.transform.forward;
        menu.transform.localScale = Vector3.one * 0.2f;
    }

    public void ShowAddRolesMenu(StolperwegeWordObject word)
    {
        CircleMenu menu = (((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")))).GetComponent<CircleMenu>();

        Hashtable types = new Hashtable();
        foreach (ArgumentRole role in ArgumentRole.Roles)
        {
            if (int.Parse((string)role.ID) <= 100 && (!(rolesMendatory.Contains(role) || rolesOptional.Contains(role))))
                types.Add(role.Value, role);
        }

        menu.Init((string key, object o) =>
        {
            ArgumentRole role = (ArgumentRole)o;

            ArgumentRoleObject roleObj =  AddRole(role, true);

            Referent.AddStolperwegeRelation(role, mendatoryRole, true);

            StartCoroutine(ConnectRoleWithWord(roleObj, word));

        }, types);

        StolperwegeHelper.PlaceInFrontOfUser(menu.transform, 0.5f);
        menu.transform.localScale = Vector3.one * 0.2f;
    }

    private IEnumerator ConnectRoleWithWord(ArgumentRoleObject roleObject, StolperwegeWordObject wordObj)
    {
        if(wordObj.ConnectedReferent == null)
        {
            yield return wordObj.CreateDRForWord();

            if (wordObj.ConnectedReferent == null) yield break;
        }

        if (roleObject.Term == null)
        {
            yield return roleObject.SetTerm(wordObj.ConnectedReferent);
            wordObj.SubText.MainTextObject.AddArgumentRole(roleObject);
        }
            

    }

    private void InitRoles()
    {
        if(mendatoryRole.title == null)
        {
            mendatoryRole = new StolperwegeInterface.RelationType();
            optionalRole = new StolperwegeInterface.RelationType();
            foreach (StolperwegeInterface.RelationType type in Referent.getRelationTypes())
            {
                if (type.id.Contains("mendatoryRole"))
                    mendatoryRole = type;
                if (type.id.Contains("optionalRole"))
                    optionalRole = type;
            }
        }
        
            

        rolesMendatory = Referent.GetRelatedElementsByType(mendatoryRole);
        rolesOptional = Referent.GetRelatedElementsByType(optionalRole);

        InitRoleObjects(rolesMendatory, true);
        InitRoleObjects(rolesOptional, false);

        if(((Predicate)Referent).proposition != null && GetComponentInChildren<PropositionObject>() == null)
        {
            if (((Predicate)Referent).proposition.StolperwegeObject == null) ((Predicate)Referent).proposition.Draw();
            GameObject propObject = ((Predicate)Referent).proposition.StolperwegeObject.gameObject;

            propObject.transform.parent = transform;
            propObject.transform.localPosition = Vector3.left * 1.25f;
        }

        if (RelatedText == null && ((Predicate)Referent).text != null)
            LinkToText(((Predicate)Referent).text);

    }

    private int i = 1;

    private void InitRoleObjects(HashSet<StolperwegeElement> roles, bool mendatory)
    {        
        foreach (StolperwegeElement role in roles)
        {
            AddRole((ArgumentRole)role, mendatory);
        }
    }

    public void SetColor(Color c)
    {
        c.a = 102f / 255f;
        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.gameObject == Icon.gameObject) continue;

            renderer.material.color = c;
        }

        
        GetComponent<MeshRenderer>().material.color = c;
    }

    private ArgumentRoleObject AddRole(ArgumentRole role, bool mendatory)
    {
        GameObject roleObject = (GameObject)GameObject.Instantiate(rolePrefab);

        roleObject.transform.parent = transform;
        roleObject.transform.localPosition = Vector3.right * 1.25f * i++;
        roleObject.transform.localRotation = Quaternion.identity;
        roleObject.GetComponent<ArgumentRoleObject>().Role = role;
        roleObject.GetComponent<ArgumentRoleObject>().Mendatory = mendatory;
        roleObject.GetComponent<ArgumentRoleObject>().Predicate = (Predicate)Referent;

        return roleObject.GetComponent<ArgumentRoleObject>();
    }

    private void Update()
    {
        if(Icon!= null && Label != null)
        {
            Icon.transform.rotation = Quaternion.LookRotation(Vector3.down, -Label.transform.forward);
        }
    }
}
