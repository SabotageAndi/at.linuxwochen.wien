Feature: Menu
	

Scenario: Standard Menu when no conference is selected
	Given no conference is selected
	When the menu is shown
	Then following menu items are shown:
		| Title       |
		| Home        |
		| Conferences |
		| About       |