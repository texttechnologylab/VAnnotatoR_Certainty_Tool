using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FT = IsoSpatialEntity.FormType;
using DT = IsoSpatialEntity.DimensionType;
using CTV = IsoLocationPlace.CTV;
using CT = IsoLocationPlace.ContinentType;
using System;
using System.Linq;

public class RadialLinkMenuData
{
    public enum MenuType { Link, SpatialEntity, Form, Dimension, Ctv, Continent, OLinkType, QsLinkType, 
        IsoEntities, IsoLinks, IsoSRelationTypes, IsoMotionType, IsoMotionClass, IsoMotionSense,
        IsoSpatialEntityTypes, IsoPlaceTypes, IsoPathTypes,
        IsoSpatialDimension, IsoSpatialForm, IsoPlaceCTV, IsoPlaceContinent
    }

    /*
     * Link: Linking of ShapeNetObjects
     * SpatialEntity: SpatialEntity Types
     * Signal_Event: Event and Signaöl Types (verwaltet ...)
     * EntityType: SpatialEntity-types 
     * IsoEntities: Alle IsoEntitäten zur Annotation im Textfenster.
     * ... Rest dürfte selbsterklärend sein
     * 
     */
    private static Dictionary<MenuType, List<RadialSection>> _radialMenuMap;
    public static Dictionary<MenuType, List<RadialSection>> RadialMenuMap
    {
        get
        {
            if (_radialMenuMap == null) Init();
            return _radialMenuMap;
        }
    }

    private static void Init()
    {
        _radialMenuMap = new Dictionary<MenuType, List<RadialSection>>();
        // Link menu
        #region
        //MainSection
        List<RadialSection> linkMenuData = new List<RadialSection>();

        RadialSection qsLinkSection = new RadialSection("QsLink", "Qualitative Spatial Link", null);


        RadialSection oLinkSection = new RadialSection("OLink", "Orientation Link", null);

        linkMenuData.Add(null);
        linkMenuData.Add(qsLinkSection);
        linkMenuData.Add(oLinkSection);

        // QsLink Section 
        List<RadialSection> qsLinkSections = new List<RadialSection>()
        {
            new RadialSection("DC", "QsLink", null), new RadialSection("EC", "QsLink", null),
            new RadialSection("PO", "QsLink", null), new RadialSection("TPP", "QsLink", null),
            new RadialSection("NTTP", "QsLink", null), new RadialSection("EQ", "QsLink", null),
            new RadialSection("IN", "QsLink", null), new RadialSection("RETURN", "to MainMenu", linkMenuData)
    };

        //oLinkSection
        List<RadialSection> oLinkSections = new List<RadialSection>()
        {
            new RadialSection("Other", "OLink", null, RadialInputType.Keyboard), new RadialSection("On", "OLink", null),
            new RadialSection("Above", "OLink", null), new RadialSection("Under", "OLink", null), 
            new RadialSection("Next To", "OLink", null), new RadialSection("Behind", "OLink", null), 
            new RadialSection("In Front of", "OLink", null), new RadialSection("RETURN", "to MainMenu", linkMenuData)
    };

        qsLinkSection.ChildSections = qsLinkSections;
        oLinkSection.ChildSections = oLinkSections;

        _radialMenuMap.Add(MenuType.OLinkType, oLinkSections);
        _radialMenuMap.Add(MenuType.QsLinkType, qsLinkSections);
        _radialMenuMap.Add(MenuType.Link, linkMenuData);
        #endregion

        // SpatialEntity menu
        #region
        List<RadialSection> spatialSection = new List<RadialSection>()
        {
            new RadialSection(IsoSpatialEntity.PrettyName, "", null, AnnotationTypes.SPATIAL_ENTITY), 
            new RadialSection(IsoLocationPath.PrettyName, IsoLocationPath.Description, null, AnnotationTypes.PATH),
            new RadialSection(IsoLocationPlace.PrettyName, IsoLocationPlace.Description, null, AnnotationTypes.PLACE),
            new RadialSection(IsoEventPath.PrettyName, IsoEventPath.Description, null, AnnotationTypes.EVENT_PATH)
        };

        _radialMenuMap.Add(MenuType.SpatialEntity, spatialSection);
        #endregion

        string prettyTitle;


        // SpatialEntity-dimensions menu
        #region
        List<RadialSection> dimensionSection = new List<RadialSection>();
        List<DT> dtValues = new List<DT>(Enum.GetValues(typeof(DT)).Cast<DT>());
        foreach (DT value in dtValues)
        {
            prettyTitle = char.ToUpper(value.ToString()[0]) + value.ToString().Substring(1);
            dimensionSection.Add(new RadialSection(prettyTitle, null, null, value));
        }

        _radialMenuMap.Add(MenuType.Dimension, dimensionSection);
        #endregion

        // SpatialEntity-forms menu
        #region
        List<RadialSection> formSection = new List<RadialSection>();
        prettyTitle = char.ToUpper(FT.none.ToString()[0]) + FT.none.ToString().Substring(1);
        formSection.Add(new RadialSection(prettyTitle, null, null, FT.none));
        formSection.Add(new RadialSection(FT.nam.ToString(), "By name", null, FT.nam));
        formSection.Add(new RadialSection(FT.nom.ToString(), "Nominally", null, FT.nam));

        _radialMenuMap.Add(MenuType.Form, formSection);
        #endregion

        // Ctv
        #region
        List<RadialSection> ctvSection = new List<RadialSection>();
        List<CTV> ctvValues = new List<CTV>(Enum.GetValues(typeof(CTV)).Cast<CTV>());
        foreach (CTV value in ctvValues)
        {
            prettyTitle = char.ToUpper(value.ToString()[0]) + value.ToString().Substring(1);
            ctvSection.Add(new RadialSection(prettyTitle, null, null, value));
        }

        _radialMenuMap.Add(MenuType.Ctv, ctvSection);
        #endregion

        // Continent
        #region
        List<RadialSection> continentSection = new List<RadialSection>();
        List<CT> continentValues = new List<CT>(Enum.GetValues(typeof(CT)).Cast<CT>());
        foreach (CT value in continentValues)
        {
            continentSection.Add(new RadialSection(value.ToString(), IsoLocationPlace.ContinentFormatMap[value], null, value));
        }

        _radialMenuMap.Add(MenuType.Continent, continentSection);
        #endregion


        Init_IsoLinks();

        Init_IsoEntities();

        Init_SRelationTypes();

        Init_MotionTypes();

        Init_MotionClasses();

        Init_MotionSenses();

        //////////////////////
        
        Init_SpatialEntityTypes();

        Init_LocationTypes();

        Init_PathTypes();

        Init_SpatialDimensions();

        Init_SpatialForm();

        /////
        
        Init_PlaceCTV();

        Init_PlaceContinent();
    }



    private static void Init_IsoLinks()
    {
        List<RadialSection> linkingSection = new List<RadialSection>();


        // QsLink Section 
        List<RadialSection> qsLinkingSections = new List<RadialSection>()
        {
            new RadialSection("DC", "QsLink", null, AnnotationTypes.QSLINK), new RadialSection("EC", "QsLink", null, AnnotationTypes.QSLINK),
            new RadialSection("PO", "QsLink", null, AnnotationTypes.QSLINK), new RadialSection("TPP", "QsLink", null, AnnotationTypes.QSLINK),
            new RadialSection("RETURN", "to MainMenu", linkingSection), new RadialSection("NTTP", "QsLink", null, AnnotationTypes.QSLINK),
            new RadialSection("EQ", "QsLink", null, AnnotationTypes.QSLINK),new RadialSection("IN", "QsLink", null, AnnotationTypes.QSLINK)
        };

        List<RadialSection> mLinkingSections = new List<RadialSection>()
        {
            new RadialSection("Other", "MLink", null, AnnotationTypes.MLINK), new RadialSection("distance", "MLink", null, AnnotationTypes.MLINK),
            new RadialSection("length", "MLink", null, AnnotationTypes.MLINK), new RadialSection("width", "MLink", null, AnnotationTypes.MLINK),
            new RadialSection("RETURN", "to MainMenu", linkingSection), new RadialSection("height", "MLink", null, AnnotationTypes.MLINK),
            new RadialSection("generalDimension", "MLink", null, AnnotationTypes.MLINK), null
        };

        List<RadialSection> srLinkingSections = new List<RadialSection>()
        {
            new RadialSection("Other", "SRLink", null, AnnotationTypes.SR_LINK), new RadialSection("Arg0", "SRLink", null, AnnotationTypes.SR_LINK),
            new RadialSection("Arg1", "SRLink", null, AnnotationTypes.SR_LINK), new RadialSection("Arg2", "SRLink", null, AnnotationTypes.SR_LINK),
            new RadialSection("RETURN", "to MainMenu", linkingSection), new RadialSection("Arg3", "SRLink", null, AnnotationTypes.SR_LINK),
            new RadialSection("Arg4", "SRLink", null, AnnotationTypes.SR_LINK), new RadialSection("Arg5", "SRLink", null, AnnotationTypes.SR_LINK)
        };

        List<RadialSection> metaLinkingSections = new List<RadialSection>()
        {
            new RadialSection("Other", "SRLink", null, AnnotationTypes.META_LINK), new RadialSection("Coreference", "metaLink", null, AnnotationTypes.META_LINK),
            new RadialSection("SubCoreference", "metaLink", null, AnnotationTypes.META_LINK), new RadialSection("SplitCoreference", "metaLink", null, AnnotationTypes.META_LINK),
            new RadialSection("RETURN", "to MainMenu", linkingSection), null,
            null, new RadialSection("PartCoreference", "metaLink", null, AnnotationTypes.META_LINK),
        };


        //////////////////////////////////////////////////////

        List<RadialSection> oLinkingSections = new List<RadialSection>();


        List<RadialSection> oLinkingAbsoluteSections = new List<RadialSection>()
        {
            new RadialSection("North", "absolut", null, AnnotationTypes.OLINK), new RadialSection("NorthEast", "absolut", null, AnnotationTypes.OLINK),
            new RadialSection("East", "absolut", null, AnnotationTypes.OLINK), new RadialSection("SouthEast", "absolut", null, AnnotationTypes.OLINK),
            new RadialSection("South", "absolut", null, AnnotationTypes.OLINK), new RadialSection("SouthWest", "absolut", null, AnnotationTypes.OLINK),
            new RadialSection("West", "absolut", null, AnnotationTypes.OLINK), new RadialSection("NorthWest", "absolut", null, AnnotationTypes.OLINK)
        };

        List<RadialSection> oLinkingIntrinsicSections = new List<RadialSection>()
        {
            new RadialSection("Other", "intrinsic", null, AnnotationTypes.OLINK), new RadialSection("ON", "intrinsic", null, AnnotationTypes.OLINK), 
            new RadialSection("ABOVE", "intrinsic", null, AnnotationTypes.OLINK), new RadialSection("BENEATH", "intrinsic", null, AnnotationTypes.OLINK),
            new RadialSection("RETURN", "to MainMenu", oLinkingSections),  new RadialSection("BELOW", "intrinsic", null, AnnotationTypes.OLINK),
            new RadialSection("NEXT_TO", "intrinsic", null, AnnotationTypes.OLINK), new RadialSection("IN_FRONT_OF", "intrinsic", null, AnnotationTypes.OLINK), 
            // new RadialSection("BEHIND", "intrinsic", null, AnnotationTypes.OLINK), new RadialSection("NEAR", "intrinsic", null, AnnotationTypes.OLINK)
        };

        List<RadialSection> oLinkingRelativeSections = new List<RadialSection>()
        {
            new RadialSection("Other", "relative", null, AnnotationTypes.OLINK), new RadialSection("NEXT_TO", "relative", null, AnnotationTypes.OLINK),
            new RadialSection("LEFT", "relative", null, AnnotationTypes.OLINK), new RadialSection("RIGHT", "relative", null, AnnotationTypes.OLINK),
            new RadialSection("RETURN", "to MainMenu", oLinkingSections),  new RadialSection("IN_FRONT_OF", "relative", null, AnnotationTypes.OLINK),
            new RadialSection("BEHIND", "relative", null, AnnotationTypes.OLINK), new RadialSection("ACROSS", "relative", null, AnnotationTypes.OLINK)        
        };

        List<RadialSection> oLinkingUndefinedSections = new List<RadialSection>()
        {
            new RadialSection("Other", "undefined", null, AnnotationTypes.OLINK), new RadialSection("NEXT_TO", "undefined", null, AnnotationTypes.OLINK),
            new RadialSection("LEFT", "undefined", null, AnnotationTypes.OLINK), new RadialSection("RIGHT", "undefined", null, AnnotationTypes.OLINK),
            new RadialSection("RETURN", "to MainMenu", oLinkingSections),  new RadialSection("IN_FRONT_OF", "undefined", null, AnnotationTypes.OLINK),
            new RadialSection("BEHIND", "undefined", null, AnnotationTypes.OLINK), new RadialSection("ACROSS", "undefined", null, AnnotationTypes.OLINK)
        };

        oLinkingSections.Add(null);
        oLinkingSections.Add(new RadialSection("Absolut", "OLink", oLinkingAbsoluteSections));
        oLinkingSections.Add(new RadialSection("Intrinsic", "OLink", oLinkingIntrinsicSections));
        oLinkingSections.Add(null);
        oLinkingSections.Add(new RadialSection("RETURN", "to MainMenu", linkingSection));
        oLinkingSections.Add(null);
        oLinkingSections.Add(new RadialSection("Relative", "OLink", oLinkingRelativeSections));
        oLinkingSections.Add(new RadialSection("Undefined", "OLink", oLinkingUndefinedSections));


        ///////////////////////////////////////////////////////

        List<RadialSection> overwriteSections = new List<RadialSection>()
        {
            null,
            new RadialSection("3DObject", "Overwrite", null),
            null,
            null,
            new RadialSection("RETURN", "to MainMenu", linkingSection),
            null,
            null,
            new RadialSection("Entity", "Overwrite", null),
        };


        linkingSection.Add(new RadialSection("Overwrite", "Overwrite", overwriteSections));  //Titel ist wichtig. Wird wo anders als Abfrage verwendet
        linkingSection.Add(new RadialSection("QsLink", "QsLink", qsLinkingSections, AnnotationTypes.QSLINK));
        linkingSection.Add(new RadialSection("OLink", "OLink", oLinkingSections, AnnotationTypes.OLINK));
        linkingSection.Add(new RadialSection("MeasureLink", "MeasureLink", mLinkingSections, AnnotationTypes.MLINK));
        linkingSection.Add(new RadialSection("CANCEL", "Close the menu", null));
        linkingSection.Add(new RadialSection("SrLink", "SrLink", srLinkingSections, AnnotationTypes.SR_LINK));
        linkingSection.Add(new RadialSection("MetaLink", "MetaLink", metaLinkingSections, AnnotationTypes.META_LINK));
        //linkingSection.Add(new RadialSection("Search 3D Object", "Search 3D Object", null));

        _radialMenuMap.Add(MenuType.IsoLinks, linkingSection);
    }


    private static void Init_IsoEntities()
    {
        List<RadialSection> isoEntitiesSection = new List<RadialSection>();



        // SpatialEntity menu
        #region
        List<RadialSection> spatialEntitiesSection = new List<RadialSection>()
        {
            null,
            new RadialSection(IsoSpatialEntity.PrettyName, IsoSpatialEntity.Description, null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection(IsoLocation.PrettyName, IsoLocation.Description, null, AnnotationTypes.LOCATION),
            new RadialSection(IsoLocationPlace.PrettyName, IsoLocationPlace.Description, null, AnnotationTypes.PLACE),
            
            new RadialSection("RETURN", "to MainMenu",isoEntitiesSection,null),
            null,
            new RadialSection(IsoEventPath.PrettyName, IsoEventPath.Description, null, AnnotationTypes.EVENT_PATH),
            new RadialSection(IsoLocationPath.PrettyName, IsoLocationPath.Description, null, AnnotationTypes.PATH)
        };


        List<RadialSection> sRelationsSection = new List<RadialSection>()
        {
            null,
            new RadialSection("topological", "topological", null, AnnotationTypes.SRELATION),
            new RadialSection("directional", "directional", null, AnnotationTypes.SRELATION),
            new RadialSection("topoDirectional", "topoDirectional", null, AnnotationTypes.SRELATION),
            new RadialSection("CANCEL", "to MainMenu",isoEntitiesSection,null),
            new RadialSection("manner", "manner", null, AnnotationTypes.SRELATION),
            new RadialSection("goalDefining", "goalDefining", null, AnnotationTypes.SRELATION),
            new RadialSection("pathDefining", "pathDefining", null, AnnotationTypes.SRELATION)
        };


        List<RadialSection> measuresSection = new List<RadialSection>()
        {
            null,
            null,
            new RadialSection(IsoMeasure.PrettyName, IsoMeasure.Description, null, AnnotationTypes.MEASURE),
            null,
            new RadialSection("RETURN", "Close the menu",isoEntitiesSection,null),
            null,
            new RadialSection(IsoMRelation.PrettyName, IsoMRelation.Description, null, AnnotationTypes.MRELATION)
        };



        List<RadialSection> eventsSection = new List<RadialSection>()
        {
            null,
            null,
            new RadialSection(IsoMotion.PrettyName, IsoMotion.Description, null, AnnotationTypes.MOTION),
            null,
            new RadialSection("RETURN", "to MainMenu",isoEntitiesSection,null),
            null,
            new RadialSection(IsoNonMotionEvent.PrettyName, IsoNonMotionEvent.Description, null, AnnotationTypes.NON_MOTION_EVENT)
        };
        #endregion

        isoEntitiesSection.Add(new RadialSection("Multitoken", "Build Multitoken", null));
        isoEntitiesSection.Add(new RadialSection("SpatialEntities", "Spatial Entitie of any kind.", spatialEntitiesSection));
        isoEntitiesSection.Add(new RadialSection("sRelation", "Signal words of any kind.", sRelationsSection));
        isoEntitiesSection.Add(new RadialSection("Events", "Signal words of any kind.", eventsSection));
        isoEntitiesSection.Add(new RadialSection("CANCEL", "Close the menu", null));
        isoEntitiesSection.Add(new RadialSection("Measures", "Signal words of any kind.", measuresSection));
        isoEntitiesSection.Add(null);
        //isoEntitiesSection.Add(new RadialSection("Search 3D Object", "Search 3D Object", null));


        _radialMenuMap.Add(MenuType.IsoEntities, isoEntitiesSection);
    }


    private static void Init_SRelationTypes()
    {

        List<RadialSection> sRelationsSection = new List<RadialSection>()
        {
            null,
            new RadialSection("topological", "topological", null, AnnotationTypes.SRELATION),
            new RadialSection("directional", "directional", null, AnnotationTypes.SRELATION),
            new RadialSection("topoDirectional", "topoDirectional", null, AnnotationTypes.SRELATION),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("manner", "manner", null, AnnotationTypes.SRELATION),
            new RadialSection("goalDefining", "goalDefining", null, AnnotationTypes.SRELATION),
            new RadialSection("pathDefining", "pathDefining", null, AnnotationTypes.SRELATION)
        };

        _radialMenuMap.Add(MenuType.IsoSRelationTypes, sRelationsSection);
    }

    private static void Init_MotionTypes()
    {
        List<RadialSection> motiontypeSection = new List<RadialSection>()
        {
            null,
            new RadialSection("manner", "manner", null, AnnotationTypes.MOTION),
            new RadialSection("path", "path", null, AnnotationTypes.MOTION),
            new RadialSection("compound", "compound", null, AnnotationTypes.MOTION),
            new RadialSection("CANCEL", "Close the menu",null),
        };

        _radialMenuMap.Add(MenuType.IsoMotionType, motiontypeSection);
    }

    private static void Init_MotionClasses()
    {
        List<RadialSection> motiontclassSection = new List<RadialSection>()
        {
            new RadialSection("move", "move", null, AnnotationTypes.MOTION),
            new RadialSection("moveExternal", "moveExternal", null, AnnotationTypes.MOTION),
            new RadialSection("moveInternal", "moveInternal", null, AnnotationTypes.MOTION),
            new RadialSection("leave", "leave", null, AnnotationTypes.MOTION),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("reach", "reach", null, AnnotationTypes.MOTION),
            new RadialSection("cross", "cross", null, AnnotationTypes.MOTION),
            new RadialSection("detach", "detach", null, AnnotationTypes.MOTION),
            new RadialSection("hit", "hit", null, AnnotationTypes.MOTION),
            new RadialSection("follow", "follow", null, AnnotationTypes.MOTION),
            new RadialSection("deviate", "deviate", null, AnnotationTypes.MOTION),
            new RadialSection("stay", "stay", null, AnnotationTypes.MOTION),
        };

        _radialMenuMap.Add(MenuType.IsoMotionClass, motiontclassSection);
    }


    private static void Init_MotionSenses()
    {
        List<RadialSection> motiontsenseSection = new List<RadialSection>()
        {
           null,
            new RadialSection("literal", "literal", null, AnnotationTypes.MOTION),
            new RadialSection("fictive", "fictive", null, AnnotationTypes.MOTION),
            new RadialSection("intrinsicChange", "intrinsicChange", null, AnnotationTypes.MOTION),
            new RadialSection("CANCEL", "Close the menu",null),

        };

        _radialMenuMap.Add(MenuType.IsoMotionSense, motiontsenseSection);
    }



    private static void Init_SpatialEntityTypes()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("Other", "Other", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("facility", "facility", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("vehicle", "vehicle", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("person", "person", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("dynamicEvent", "dynamicEvent", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("artefact", "artefact", null, AnnotationTypes.SPATIAL_ENTITY)
        };

        _radialMenuMap.Add(MenuType.IsoSpatialEntityTypes, entitytypes);
    }

    private static void Init_LocationTypes()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("Other", "Other", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("water", "water", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("celestial", "celestial", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("civil", "civil", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("country", "country", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("grid", "grid", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("latLong", "latLong", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("mtn", "mountain", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("mts", "mountain range", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("postalCode", "postalCode", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("postBox", "postBox", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("ppl", "populated place", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("ppla", "capital of sub-country", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("pplc", "capital of country", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("rgn", "non-political", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("state", "state", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("UTM", "UTM", null, AnnotationTypes.SPATIAL_ENTITY),
        };

        _radialMenuMap.Add(MenuType.IsoPlaceTypes, entitytypes);
    }

    private static void Init_PathTypes()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("Other", "Other", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("waterway", "waterway", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("railway", "railway", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("bridge", "bridge", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("tunnel", "tunnel", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("road", "road", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("lane", "lane", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("passage", "passage", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("trail", "trail", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("boundary", "boundary", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("barrier", "barrier", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("margin", "margin", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("row", "row", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("conduit", "conduit", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("filament", "filament", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("mtn", "mtn", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("mts", "mts", null, AnnotationTypes.SPATIAL_ENTITY),
        };

        _radialMenuMap.Add(MenuType.IsoPathTypes, entitytypes);
    }


    private static void Init_SpatialDimensions()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("point", "point", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("line", "line", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("area", "area", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("volume", "volume", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("CANCEL", "Close the menu",null)
        };

        _radialMenuMap.Add(MenuType.IsoSpatialDimension, entitytypes);
    }

    private static void Init_SpatialForm()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("nam", "nam", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("nom", "nom", null, AnnotationTypes.SPATIAL_ENTITY),
            null,
            null,
            new RadialSection("CANCEL", "Close the menu",null)
        };

        _radialMenuMap.Add(MenuType.IsoSpatialForm, entitytypes);
    }


    private static void Init_PlaceCTV()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("Other", "Other", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("city", "city", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("town", "town", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("village", "village", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("CANCEL", "Close the menu",null)
        };

        _radialMenuMap.Add(MenuType.IsoPlaceCTV, entitytypes);
    }

    private static void Init_PlaceContinent()
    {
        List<RadialSection> entitytypes = new List<RadialSection>()
        {
            new RadialSection("None", "-", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("AF", "Africe", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("AN", "Antarctica", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("AS", "Asia", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("CANCEL", "Close the menu",null),
            new RadialSection("EU", "Europe", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("NA", "North America", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("SA", "South America", null, AnnotationTypes.SPATIAL_ENTITY),
            new RadialSection("OC", "Oceania", null, AnnotationTypes.SPATIAL_ENTITY),
        };
        _radialMenuMap.Add(MenuType.IsoPlaceContinent, entitytypes);
    }
}
