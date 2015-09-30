Feature: Menu
	

Scenario: Add two numbers
	Given no conference is selected
	When the menu is shown
	Then following menu items are shown:
		| Title       |
		| Home        |
		| Conferences |
		| About       |