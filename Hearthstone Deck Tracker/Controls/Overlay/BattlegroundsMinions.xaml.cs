﻿using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsMinions : UserControl
	{
		private Lazy<BattlegroundsDb> _db = new Lazy<BattlegroundsDb>();
		private readonly List<BattlegroundsTier> _tierIcons = new List<BattlegroundsTier>();

		public int ActiveTier { get; set; }
		public ObservableCollection<BattlegroundsCardsGroup> Groups { get; set; } = new ObservableCollection<BattlegroundsCardsGroup>();

		public BattlegroundsMinions()
		{
			InitializeComponent();
			_tierIcons = BgTierIcons.Children.Cast<BattlegroundsTier>().ToList();
		}

		private void BgTier_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var tier = ((BattlegroundsTier)sender).Tier;
			Update(tier == ActiveTier ? 0 : tier);
		}

		public void Reset()
		{
			Update(0);
		}

		private bool AddOrUpdateBgCardGroup(string title, List<Hearthstone.Card> cards)
		{
			var addedNew = false;
			var existing = Groups.FirstOrDefault(x => x.Title == title);
			if(existing == null)
			{
				existing = new BattlegroundsCardsGroup() { Title = title };
				Groups.Add(existing);
				addedNew = true;
			}
			var sortedCards = cards
				.OrderBy(x => x.LocalizedName)
				.ToList();
			existing.UpdateCards(sortedCards);
			return addedNew;
		}

		private void Update(int tier)
		{
			if (ActiveTier == tier)
				return;
			ActiveTier = tier;
			foreach(var item in _tierIcons)
				item.Active = tier == item.Tier;
			if(tier < 1 || tier > 6)
			{
				for(var i = 0; i < 6; i++)
					_tierIcons[i].SetFaded(false);
				Groups.Clear();
				return;
			}
			for(var i = 0; i < 6; i++)
				_tierIcons[i].SetFaded(i != tier - 1);

			var resort = false;
			foreach(var race in _db.Value.Races)
			{
				var title = race == Race.INVALID ? "Other" : HearthDbConverter.RaceConverter(race);
				var cards = _db.Value.GetCards(tier, race);
				if(cards.Count == 0)
					Groups.FirstOrDefault(x => x.Title == title)?.Hide();
				else
					resort |= AddOrUpdateBgCardGroup(title, cards);
			}

			if (resort)
			{
				var items = Groups.ToList()
					.OrderBy(x => string.IsNullOrEmpty(x.Title))
					.ThenBy(x => x.Title);
				foreach(var item in items)
				{
					Groups.Remove(item);
					Groups.Add(item);
				}
			}
		}
	}
}
