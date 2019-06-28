﻿using Microsoft.Xna.Framework;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SummonersAssociation.Items
{
	public class MinionHistoryBook : ModItem
	{
		public override bool CloneNewInstances => true;

		public List<ItemModel> history = new List<ItemModel>();

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion History Book");
			//TODO
			Tooltip.SetDefault("TODO"
				+ "\nLeft click summon minions based on history"
				+ "\nRight click to open an UI"
				+ "\nLeft/Right click on the item icons to adjust the summon count");
		}

		public override void SetDefaults() {
			item.width = 28;
			item.height = 30;
			item.maxStack = 1;
			item.rare = 4;
			item.useAnimation = 16;
			item.useTime = 16;
			item.useStyle = 4;
			item.UseSound = SoundID.Item46;
			item.value = Item.sellPrice(silver: 10);
		}

		public override ModItem Clone() {
			var clone = (MinionHistoryBook)base.Clone();
			clone.history = history.ConvertAll((itemModel) => new ItemModel(itemModel));
			return clone;
		}

		public override TagCompound Save() {
			return new TagCompound {
				{nameof(history), history}
			};
		}

		public override void Load(TagCompound tag) {
			//Load and remove unloaded items from history
			history = tag.GetList<ItemModel>(nameof(history)).Where(x => x.ItemType != ItemID.Count).ToList();
		}

		public override void NetRecieve(BinaryReader reader) {
			int length = reader.ReadByte();
			for (int i = 0; i < length; i++) {
				history[i].NetRecieve(reader);
			}
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)history.Count);
			for (int i = 0; i < history.Count; i++) {
				history[i].NetSend(writer);
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			List<ItemModel> localHistory = HistoryBookUI.MergeHistoryIntoInventory(this);
			if (localHistory.Count > 0) {
				for (int i = 0; i < localHistory.Count; i++) {
					ItemModel itemModel = localHistory[i];
					//Only show in the tooltip if theres a number assigned
					if (itemModel.SummonCount > 0) {
						tooltips.Add(new TooltipLine(mod, "ItemModel", itemModel.Name + ": " + itemModel.SummonCount) {
							overrideColor = itemModel.Active ? Color.White : Color.Red
						});
					}
				}
			}
			else {
				tooltips.Add(new TooltipLine(mod, "None", "No summon history specified"));
			}
		}

		public override bool UseItem(Player player) {
			//Here would be code to summon the history
			//clueless how to make it return false and still only run this code once without relying on animation duration
			//if (player.itemTime == 0 && player.itemAnimation == item.useAnimation - 1) Main.NewText(currentMinionWeaponType);

			if (player.whoAmI == Main.myPlayer) {
				for (int i = 0; i < 1000; i++) {
					Projectile p = Main.projectile[i];
					if (p.active && p.owner == Main.myPlayer && p.minion) {
						p.Kill();
					}
				}

				var SAPlayer = player.GetModPlayer<SAPlayer>();
				SAPlayer.pendingCasts.Clear();
				foreach (var item in history) {
					for (int i = 0; i < item.SummonCount; i++) {
						SAPlayer.pendingCasts.Enqueue(new System.Tuple<int, int>(item.ItemType, 1));
					}
				}
			}
			return true;
		}
	}
}
