using Content.Shared._ST14.Localization; // ST14
using Content.Shared.Dataset;
using Content.Shared.Random.Helpers;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.RandomMetadata;

public sealed partial class RandomMetadataSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MetaDataSystem _metaData = default!;

    private readonly List<(string, object)> _outputSegments = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomMetadataComponent, MapInitEvent>(OnMapInit);
    }

    // This is done on map init so that map-placed entities have it randomized each time the map loads, for fun.
    private void OnMapInit(EntityUid uid, RandomMetadataComponent component, MapInitEvent args)
    {
        var meta = MetaData(uid);

        if (component.NameSegments != null)
        {
            // ST14-START
            var st14Parts = new List<string>();
            _metaData.SetEntityName(uid, GetLocalizedFromSegments(component.NameSegments, component.NameFormat, st14Parts), meta);

            var st14Localized = EnsureComp<RandomMetadataNameComponent>(uid);
            st14Localized.Format = component.NameFormat;
            st14Localized.Parts = st14Parts;
            Dirty(uid, st14Localized);
            // ST14-STOP
        }

        if (component.DescriptionSegments != null)
        {
            _metaData.SetEntityDescription(uid,
                GetRandomFromSegments(component.DescriptionSegments, component.DescriptionFormat), meta);
        }
    }

    /// <summary>
    /// Generates a random string from segments and a separator.
    /// </summary>
    /// <param name="segments">The segments that it will be generated from</param>
    /// <param name="format">The format string used to combine the segments.</param>
    /// <returns>The newly generated string</returns>
    [PublicAPI]
    public string GetRandomFromSegments(List<ProtoId<LocalizedDatasetPrototype>> segments, LocId format)
    {
        _outputSegments.Clear();
        for (var i = 0; i < segments.Count; ++i)
        {
            var localizedProto = ProtoMan.Index(segments[i]);
            _outputSegments.Add(($"part{i}", _random.Pick(localizedProto)));
        }

        return Loc.GetString(format, _outputSegments.ToArray());
    }

    // ST14-START
    private string GetLocalizedFromSegments(List<ProtoId<LocalizedDatasetPrototype>> segments, LocId format, List<string> picked)
    {
        picked.Clear();
        _outputSegments.Clear();

        for (var i = 0; i < segments.Count; ++i)
        {
            var localizedProto = ProtoMan.Index(segments[i]);
            var locId = localizedProto.Values[_random.Next(localizedProto.Values.Count)];

            picked.Add(locId);
            _outputSegments.Add(($"part{i}", Loc.GetString(locId)));
        }

        return Loc.GetString(format, _outputSegments.ToArray());
    }
    // ST14-STOP
}
