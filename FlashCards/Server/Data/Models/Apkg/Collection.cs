using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models.Apkg
{
	//col contains a single row that holds various information about the collection
	[Table("col")]
	public class Collection
	{
		[Key]
		[Column("id")]
		public long Id { get; set; } // arbitrary number since there is only one row
		[Column("crt")]
		public long CreationTime { get; set; } // timestamp of the creation date in second. It's correct up to the day. For V1 scheduler, the hour corresponds to starting a new day. By default, new day is 4.
		[Column("mod")]
		public long ModifiedTime { get; set; } // last modified in milliseconds
		[Column("scm")]
		public long SchemaModifiedTime { get; set; } // schema mod time: time when "schema" was modified. 
													 // If server scm is different from the client scm a full-sync is required
		[Column("ver")]
		public long Version { get; set; }
		[Column("dty")]
		public long Dirty { get; set; } // dirty: unused, set to 0
		[Column("usn")]
		public long UpdateSequenceNumber { get; set; } // update sequence number: used for finding diffs when syncing.
													   // See usn in cards table for more details.
		[Column("ls")]
		public long LastSyncTime { get; set; }
		[Column("conf", TypeName = "text")]
		public string Config { get; set; } = string.Empty; // json object containing configuration options that are synced. Described below in "configuration JSONObjects"
		[Column("models", TypeName = "text")]
		public string Models { get; set; } = string.Empty; // json object of json object(s) representing the models (aka Note types) 
														   // keys of this object are strings containing integers: "creation time in epoch milliseconds" of the models
														   // values of this object are other json objects of the form described below in "Models JSONObjects"
		[Column("decks", TypeName = "text")]
		public string Decks { get; set; } = string.Empty; // json object of json object(s) representing the deck(s)
														  // keys of this object are strings containing integers: "deck creation time in epoch milliseconds" for most decks, "1" for the default deck
														  // values of this object are other json objects of the form described below in "Decks JSONObjects"
		[Column("dconf", TypeName = "text")]
		public string DeckConfig { get; set; } = string.Empty; // json object of json object(s) representing the options group(s) for decks
															   // keys of this object are strings containing integers: "options group creation time in epoch milliseconds" for most groups, "1" for the default option group
															   // values of this object are other json objects of the form described below in "DConf JSONObjects"
		[Column("tags", TypeName = "text")]
		public string Tags { get; set; } = string.Empty; // a cache of tags used in the collection (This list is displayed in the browser. Potentially at other place)

		/* configuration JSONObject
		 * Here is an annotated description of the JSONObject in the conf field of the col table when the collection is started. More values may be added to it by any add-on. Unlike the models, decks, and dconf JSONObjects, there should be only one conf JSONObject per collection.
		 {
		     "curDeck": "The id (as int) of the last deck selected (during review, adding card, changing the deck of a card)",
		     "activeDecks": "The list containing the current deck id and its descendant (as ints)",
		     "newSpread": "In which order to view to review the cards. This can be selected in Preferences>Basic. Possible values are:
		       0 -- NEW_CARDS_DISTRIBUTE (Mix new cards and reviews)
		       1 -- NEW_CARDS_LAST (see new cards after review)
		       2 -- NEW_CARDS_FIRST (See new card before review)",
		     "collapseTime": "'Preferences>Basic>Learn ahead limit'*60. If there is no more card to review now but next card in learning is in less than collapseTime second, show it now.
		     If there are no other card to review, then we can review cards in learning in advance if they are due in less than this number of seconds.",
		     "timeLim": "'Preferences>Basic>Timebox time limit'*60. Each time this number of second elapse, anki tell you how many card you reviewed.",
		     "estTimes": "'Preferences>Basic>Show next review time above answer buttons'. A Boolean."
		     "dueCounts": "'Preferences>Basic>Show remaining card count during review'. A Boolean."
		     "curModel": "Id (as string) of the last note type (a.k.a. model) used (i.e. either when creating a note, or changing the note type of a note).",
		     "nextPos": "This is the highest value of a due value of a new card. It allows to decide the due number to give to the next note created. (This is useful to ensure that cards are seen in order in which they are added.",
		     "sortType": "A string representing how the browser must be sorted. Its value should be one of the possible value of 'aqt.browsers.DataModel.activeCols' (or equivalently of 'activeCols'  but not any of ('question', 'answer', 'template', 'deck', 'note', 'noteTags')",
		     "sortBackwards": "A Boolean stating whether the browser sorting must be in increasing or decreasing order",
		     "addToCur": "A Boolean. True for 'When adding, default to current deck' in Preferences>Basic. False for 'Change deck depending on note type'.",
		     "dayLearnFirst": "A Boolean. It corresponds to the option 'Show learning cards with larger steps before reviews'. But this option does not seems to appear in the preference box",
		     "newBury": "A Boolean. Always set to true and not read anywhere in the code but at the place where it is set to True if it is not already true. Hence probably quite useful.",
		 
		     "lastUnburied":"The date of the last time the scheduler was initialized or reset. If it's not today, then buried notes must be unburied. This is not in the json until scheduler is used once.",
		     "activeCols":"the list of name of columns to show in the browser. Possible values are listed in aqt.browser.Browser.setupColumns. They are:
		     'question' -- the browser column'Question',
		     'answer' -- the browser column'Answer',
		     'template' -- the browser column'Card',
		     'deck' -- the browser column'Deck',
		     'noteFld' -- the browser column'Sort Field',
		     'noteCrt' -- the browser column'Created',
		     'noteMod' -- the browser column'Edited',
		     'cardMod' -- the browser column'Changed',
		     'cardDue' -- the browser column'Due',
		     'cardIvl' -- the browser column'Interval',
		     'cardEase' -- the browser column'Ease',
		     'cardReps' -- the browser column'Reviews',
		     'cardLapses' -- the browser column'Lapses',
		     'noteTags' -- the browser column'Tags',
		     'note' -- the browser column'Note',
		     The default columns are: noteFld, template, cardDue and deck
		     This is not in the json at creation. It's added when the browser is open.
		 }
		 */

		/* Models JSONObjects
		 * Here is an annotated description of the JSONObjects in the models field of the col table. Each object is the value of a key that's a model id (epoch time in milliseconds):
		    {
			"model id (epoch time in milliseconds)" :
			  {
			    css : "CSS, shared for all templates",
			    did :
			        "Long specifying the id of the deck that cards are added to by default",
			    flds : [
			             "JSONArray containing object for each field in the model as follows:",
			             {
			               font : "display font",
			               media : "array of media. appears to be unused",
			               name : "field name",
			               ord : "ordinal of the field - goes from 0 to num fields -1",
			               rtl : "boolean, right-to-left script",
			               size : "font size",
			               sticky : "sticky fields retain the value that was last added 
			                           when adding new notes"
			             }
			           ],
			    id : "model ID, matches notes.mid",
			    latexPost : "String added to end of LaTeX expressions (usually \\end{document})",
			    latexPre : "preamble for LaTeX expressions",
			    mod : "modification time in seconds",
			    name : "model name",
			    req : [
			            "req is unused in modern clients. May exist for backwards compatibility. 
			             https://forums.ankiweb.net/t/is-req-still-used-or-present/9977
			             AnkiDroid 2.14 uses it, AnkiDroid 2.15 does not use it but still generates it.
			             Array of arrays describing, for each template T, which fields are required to generate T.
			             The array is of the form [T,string,list], where:
			             -  T is the ordinal of the template. 
			             - The string is 'none', 'all' or 'any'. 
			             - The list contains ordinal of fields, in increasing order.
			             The meaning is as follows:
			             - if the string is 'none', then no cards are generated for this template. The list should be empty.
			             - if the string is 'all' then the card is generated only if each field of the list are filled
			             - if the string is 'any', then the card is generated if any of the field of the list is filled.
			
			             The algorithm to decide how to compute req from the template is explained on: 
			             https://github.com/Arthur-Milchior/anki/blob/commented/documentation//templates_generation_rules.md"
			          ],
			    sortf : "Integer specifying which field is used for sorting in the browser",
			    tags : "Anki saves the tags of the last added note to the current model, use an empty array []",
			    tmpls : [
			              "JSONArray containing object of CardTemplate for each card in model",
			              {
			                afmt : "answer template string",
			                bafmt : "browser answer format: 
			                          used for displaying answer in browser",
			                bqfmt : "browser question format: 
			                          used for displaying question in browser",
			                did : "deck override (null by default)",
			                name : "template name",
			                ord : "template number, see flds",
			                qfmt : "question format string"
			              }
			            ],
			    type : "Integer specifying what type of model. 0 for standard, 1 for cloze",
			    usn : "usn: Update sequence number: used in same way as other usn vales in db",
			    vers : "Legacy version number (unused), use an empty array []"
			  }
			}
		 */

		/* Decks JSONObjects
		 * Here is an annotated description of the JSONObjects in the decks field of the col table:
		 {
		 "deck id (creation time in epoch milliseconds for most decks, '1' for the default deck)"
			{
			name: "name of deck", 
			extendRev: "extended review card limit (for custom study)
						Potentially absent, in this case it's considered to be 10 by aqt.customstudy", 
			usn: "usn: Update sequence number: used in same way as other usn vales in db", 
			collapsed: "true when deck is collapsed", 
			browserCollapsed: "true when deck collapsed in browser", 
			newToday/revToday/lrnToday : two number array.
										 First one is the number of days that have passed between the collection was created and the deck was last updated
										 The second one is equal to the number of cards seen today in this deck minus the number of new cards in custom study today.
										 BEWARE, it's changed in anki.sched(v2).Scheduler._updateStats and anki.sched(v2).Scheduler._updateCutoff.update  but can't be found by grepping 'newToday', because it's instead written as type+"Today" with type which may be new/rev/lrnToday    
			timeToday: "two number array used somehow for custom study. Currently unused in the code", 
			dyn: "1 if dynamic (AKA filtered) deck", 
			extendNew: "extended new card limit (for custom study). 
						Potentially absent, in this case it's considered to be 10 by aqt.customstudy", 
			conf: "id of option group from dconf in `col` table. Or absent if the deck is dynamic. 
				  Its absent in filtered deck", 
			id: "deck ID (automatically generated long)", 
			mod: "last modification time", 
			desc: "deck description"
			}
		 }
		 */

		/* DConf JSONObjects
		 * Here is an annotated description of the JSONObjects in the dconf field of the col.decks table:
		 {
		 "deck config id (creation time in epoch milliseconds for most option groups, '1' for the default option group)" :
		   {
		         autoplay : "whether the audio associated to a question should be played when the question is shown"
		         dyn : "Whether this deck is dynamic. Not present by default in decks.py"
		         id : "deck ID (automatically generated long). Not present by default in decks.py"
		         lapse : {
		             "The configuration for lapse cards."
		             delays : "The list of successive delay between the learning steps of the new cards, as explained in the manual."
		             leechAction : "What to do to leech cards. 0 for suspend, 1 for mark. Numbers according to the order in which the choices appear in aqt/dconf.ui"
		             leechFails : "the number of lapses authorized before doing leechAction."
		             minInt: "a lower limit to the new interval after a leech"
		             mult : "percent by which to multiply the current interval when a card goes has lapsed"
		         }
		         maxTaken : "The number of seconds after which to stop the timer"
		         mod : "Last modification time"
		         name : "The name of the configuration"
		         new : {
		             "The configuration for new cards."
		             bury : "Whether to bury cards related to new cards answered"
		             delays : "The list of successive delay between the learning steps of the new cards, as explained in the manual."
		             initialFactor : "The initial ease factor"
		             ints : "The list of delays according to the button pressed while leaving the learning mode. Good, easy and unused. In the GUI, the first two elements corresponds to Graduating Interval and Easy interval"
		             order : "In which order new cards must be shown. NEW_CARDS_RANDOM = 0 and NEW_CARDS_DUE = 1."
		             perDay : "Maximal number of new cards shown per day."
		             separate : "Seems to be unused in the code."
		                 
		         }
		         replayq : "whether the audio associated to a question should be played when the answer is shown"
		         rev : {
		             "The configuration for review cards."
		             bury : "Whether to bury cards related to new cards answered"
		             ease4 : "the number to add to the easyness when the easy button is pressed"
		             fuzz : "The new interval is multiplied by a random number between -fuzz and fuzz"
		             ivlFct : "multiplication factor applied to the intervals Anki generates"
		             maxIvl : "the maximal interval for review"
		             minSpace : "not currently used according to decks.py code's comment"
		             perDay : "Numbers of cards to review per day"
		         }
		         timer : "whether timer should be shown (1) or not (0)"
		         usn : "See usn in cards table for details."
		   }
		 }
		 */
	}
}
