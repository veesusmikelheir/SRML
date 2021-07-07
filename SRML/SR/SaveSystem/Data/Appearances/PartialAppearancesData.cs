using HarmonyLib;
using MonomiPark.SlimeRancher.Persist;
using SRML.SR.SaveSystem.Data.Partial;
using SRML.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static SlimeAppearance;

namespace SRML.SR.SaveSystem.Data.Appearances
{
    class PartialAppearancesData : PartialData<AppearancesV01>
    {
        static SerializerPair<AppearanceSaveSet> saveSetPair = SerializerPair.GetEnumSerializerPair<AppearanceSaveSet>();
        static SerializerPair<Identifiable.Id> identifiablePair = SerializerPair.GetEnumSerializerPair<Identifiable.Id>();
        PartialDictionary<Identifiable.Id, List<SlimeAppearance.AppearanceSaveSet>> unlocksCustom = new PartialDictionary<Identifiable.Id, List<AppearanceSaveSet>>((x) => ModdedIDRegistry.IsModdedID(x.Key), identifiablePair, new SerializerPair<List<AppearanceSaveSet>>((x, y) => BinaryUtils.WriteList(x, y, saveSetPair.SerializeGeneric), (x) =>
        {
            var list = new List<AppearanceSaveSet>();
            BinaryUtils.ReadList(x, list, saveSetPair.DeserializeGeneric);
            return list;


        }));

        Dictionary<Identifiable.Id, PartialCollection<AppearanceSaveSet>> unlocksPartial = new Dictionary<Identifiable.Id, PartialCollection<AppearanceSaveSet>>();

        PartialDictionary<Identifiable.Id, AppearanceSaveSet> selections = new PartialDictionary<Identifiable.Id, AppearanceSaveSet>(x => ModdedIDRegistry.IsModdedID(x.Key) || ModdedIDRegistry.IsModdedID(x.Value), identifiablePair, saveSetPair, (x) => ModdedIDRegistry.IsModdedID(x.Key)?null:new KeyValuePair<Identifiable.Id,AppearanceSaveSet>?(new KeyValuePair<Identifiable.Id, AppearanceSaveSet>(x.Key, AppearanceSaveSet.CLASSIC)));

        PartialCollection<AppearanceSaveSet> CreatePartialList()
        {
            return new PartialCollection<AppearanceSaveSet>(ModdedIDRegistry.IsModdedID, saveSetPair);
        }

        public override void Pull(AppearancesV01 data)
        {

            unlocksCustom.Pull(data.unlocks);

            unlocksPartial.Clear();
            foreach(var v in data.unlocks)
            {
                if (v.Value.Any(ModdedIDRegistry.IsModdedID))
                {
                    var partial = CreatePartialList();
                    partial.Pull(v.Value);
                    unlocksPartial.Add(v.Key, partial);
                }
            }
            selections.Pull(data.selections);

        }

        public override void Push(AppearancesV01 data)
        {
            
            unlocksCustom.Push(data.unlocks);



            foreach(var v in unlocksPartial)
            {
                v.Value.Push(data.unlocks[v.Key]);
            }


            data.unlocks.Remove(Identifiable.Id.NONE);

            foreach (var v in data.unlocks) v.Value.RemoveAll((x) => x == AppearanceSaveSet.NONE);

            selections.Push(data.selections);

            data.selections.Remove(Identifiable.Id.NONE);
        }

        public override void Read(BinaryReader reader)
        {
            unlocksCustom.Read(reader);
            unlocksPartial.Clear();
            int count = reader.ReadInt32();
            for(int i = 0; i < count; i++)
            {
                var create = CreatePartialList();
                create.Read(reader);
                var idpair = identifiablePair.DeserializeGeneric(reader);
                unlocksPartial.Add(idpair, create);
            }

            selections.Read(reader);
        }

        public override void Write(BinaryWriter writer)
        {
            unlocksCustom.Write(writer);
            writer.Write(unlocksPartial.Count);
            foreach (var pair in unlocksPartial)
            {
                pair.Value.Write(writer);
                identifiablePair.SerializeGeneric(writer, pair.Key);
            }

            selections.Write(writer);
        }


        static PartialAppearancesData()
        {
            PartialData.RegisterPartialData<AppearancesV01>(typeof(PartialAppearancesData));
            EnumTranslator.RegisterEnumFixer<PartialAppearancesData>((x, y, z) =>
            {
                EnumTranslator.FixEnumValues(x, y, z.unlocksCustom);
                EnumTranslator.FixEnumValues(x, y, z.unlocksPartial);
                EnumTranslator.FixEnumValues(x, y, z.selections);
            });
        }
    }
}
