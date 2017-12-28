using System;
using System.Collections.Generic;

namespace Assets.Scripts.Battle.model
{
	public class Card
	{
		public string id;                   //"#001", "#002"...
		public string name;                 //"Oni Stomp", "Twin Pistons"...
		public int damage;                  //e.g. 50
		public string textEN;               //the card's descriptive text in English
		public string textRU;               //the card's descriptive text in Russian
		public int priority;                //set to 1 for nullification effects
		public string activation;           //"onAppear", "Pre", "After", "Nothing"
		public List<Func<bool>> effect;     //CardEffects.Method(), null
		public string rarity;               //"Bronze", "Silver", "Gold", "SR"
		public string type;                 //"Punch", "Kick"
		public bool limited;
		public bool unique;

		//jin only
		public string mutatedTextEN;        //colored text for mutated status in English
		public string mutatedTextRU;        //colored text for mutated status in Russian

		//only for cards in play
		public bool tenacity;               //cards with tenacity are not discarded
	}
}
