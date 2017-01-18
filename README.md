

# PrivateSnafu2ndFront
UWP release of Private Snafu

Building the Clean Copy of Private Snafu for the release
The first "Private Snafu" Windows Store app was written by me (Percy Tierney) in 2012 while I was learning to write Windows RT / Store apps for Windows RT / 8. That app was reasonably successful, with over a thousand downloads and not one single public crash. 
Since then, Windows has moved on to Windows 10 and now recommends the Universal Windows Platform (UWP) API. Instead of simply updating the current Private Snafu, I decided to write a new version for UWP from scratch. The new version leverages about half the code from the old RT version, and includes new features, primarily using JSON for data and adding multi-language support. 

Beginnings
We want a minty fresh, clean UWP app. Start Visual Studio 15 “Community” and create a new project from the Template, “Visual C#, Windows, Universal, Blank App (Universal Windows)”. This gives us a basic one page app. 
Go into Assets and replace the placeholder art with the real art.

You will need one good quality base image to generate all the Tile images. Open the “Package.appmanifest” and the “Visual Assets” tab – this shows you the minimum assets you need to create. Open “Solution Explorer” and the “Assets” folder – delete all the generic logos you find there.

Open your base image in MSPaint. Use “Image”, “Resize” from the Toolbar, uncheck “Maintain Aspect Ratio” and enter the new size. You will need 5 base pixel sized images – Square 44x44, 71x71, 150x150, 310x310 and Wide 310x150. These need to be in 100%, 200% and 400% sizes – 15 total images for the logos. In addition, you will need a “Splash” screen, Badge and Store icon, each in the 3 sizes. Total images are 24.

Create each art work and save separately, with a name like “Square44x44Logo.png”. Copy these 5 into the Project “Assets” folder. Right click on the “Solution Explorer”, “Assets” and select “Add”, “Existing Item” for the new art. In the manifest “Visual Assets” area, go to the “Square 44x44 Logo”, click the ellipses on “44 x 44 pix” and select “Square44x44Logo.png”. You will need to create these in 100, 200 and 400 scale sizes.  Once that’s done, make sure that the VS renamed version is in the Assets list and eliminate your original. Continue until all the major Logos are filled.

The Splash screen needs to be 620 x 300 pixels, and 200kb or smaller in size. One way to do this is create the 620x300 image in MSPaint, fill the background with a single color, and add an image or text to the background. If you pick an image, be sure to watch the 200kb size limit. A full background is very small, and the image can be small to stay within the file size limits.

Check that there are no “Red Cross” error flags.

Reference: https://msdn.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-app-assets

Finish the manifest
In the manifest “Application” tab, set your “Display Name” (ex, “PrivateSnafu2ndFront”), “Default Language” (“en-us”, or English in the USA), additional language (“es”, or Spanish (Espanol)) and “Description” (“Private Snafu – the 2nd Front”). 

In the “Capabilities” tab, ensure “Internet (Client)” is picked. 

In the “Packaging” tab, check the Package and Publisher Display Names, and the Version.
That completes the Manifest.

Add the Strings
Right click “Solution Explorer”, add “New Folder”, and create “Strings”. Make an “en-US” and “es” folder. Copy the respect language string resources into these. The "en-us" is the default Culture – English in the United States. The "es" is a supported Culture – Spanish anywhere. You can reference these ISO Language code tables here: http://www.lingoes.net/en/translator/langcode.htm

First Build
UWP apps can be built for ARM, x86 and x64. The “All Platforms” build is not supported for Windows 10 Store apps. Build the app and check that the Logos show up in the “All Apps” menu, on the TaskBar, and that you see your splash screen when the app starts up.

Add the Styles Dictionary
From “Solution Explorer”, add the existing item “Styles Dictionary”. Open “App.xaml” and add the <Application.Resources> section including the “StylesDictionary.xaml”. In “StylesDictionary.xaml” update the “using:” section to “using:PrivateSnafu2ndFront”. These are the app global Style definitions for the movie data title, text, graphics and error messages.

Add MovieData.cs to the project, and update the namespace to be the current project. This is an object that describes the movie data (title, text, image and movie location).

App.xaml and App.xaml.cs don’t need any updates, as all the work has been moved to MainPage.

Update MainPage.xaml and MainPage.xaml.cs

Open MainPage.xaml and replace these lines with the contents from the source MainPage.xaml (the original RT source file).
    <Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    </Grid>


Open MainPage.xaml and replace all the code below the namespace with the code from the original RT source project.

Add the PlayerPage.xaml from the source – it will also add PlayerPage.xaml.cs. Update the namespace for both.

Attempt the second build. The app should launch with the “Hamburger”, “Contact” and “About” buttons (functional) on the MainPage.

Add the rest of the Assets

Set the properties to “Content” and “Copy Always”.


On the MainPage.xaml, set the MainPage_Grid ImageSource to a new, clear Snafu image.

Advertising
The original RT advertising API is no longer supported.

Close Visual Studio and uninstall ALL previous instances of the Microsoft Advertising or “Store Monetization and Engagement SDK”. The latest version is here:

https://visualstudiogallery.msdn.microsoft.com/229b7858-2c6a-4073-886e-cbb79e851211

The setup is very fussy. You need to use test values for the ApplicationId and AdUnitId until release time, at which the actual Store Id’s must be used.

My Private Snafu webpage is http://FrostBackCrow.com/Fun. Here's the main text for the "About". You will need a separate link for each language you want to support. "PS2" has two languages, so it includes English and Spanish "About" pages.

"About Private Snafu" web site text
" On December 7, 1941, the USA was unexpectedly plunged into the middle of World War II. There was a lot of panic, confusion and plumb dumb goofs made right from the beginning. 

Hollywood to the rescue! The “Office of War Information” was set up to educate and influence the general public using whatever they could get their hands on – newspapers, magazines, radio and even comic books. Under the leadership of Brigadier General Frederick Osborn, movies were picked as the best medium, and the enlisted Service men and women as the best target. These new recruits were fresh and full of spirit, but they came from all walks of society, from field hands who could barely read to Captains of Industry used to command. 

Film Director Frank Capra was picked to lead the educational film team. He produced a series of propaganda shorts called, “Why We Fight”. Eric Knight, the writer who created the fabled film dog “Lassie”, added some animated bits to these shorts. Those cartoons got some of the biggest laughs from their audiences, so Capra looked to make a whole new series of cartoons designed to teach raw recruits what to do. Bids were put out to make these, with Walt Disney considered the likeliest winner. However, Disney wanted to keep the rights to any characters created, plus they put in a high bid. Warner Brothers’ animation Producer Leon Schlesinger bid low and won the deal, including keeping the characters in the public domain.

The cartoons would be short and done in black and white, to keep costs down. Schlesinger used his best talent from the “Looney Tunes” and Merrie Melodies” staff, with directors Friz Freleng (inspiration for “Yosemito Sam”), Chuck Jones, Frank Tashlin and Bob Clampett. The masterful Mel Blanc provided voices. Capra picked Ted Geisel (who would become “Dr. Seuss”) as the chief writer. Over the course of the series, others would add their talents.

Schlesinger decided to educate by counterexample, and came up with an “Everyman” who did it all wrong due to ignorance and laziness. Private Snafu was introduced in the 1943 “Coming!!!” which also explains his name. “SNAFU – Situation Normal, All ….. Fouled Up”. The first set of cartoons covered basic military topics like rumor control (“Rumors”), laziness (“The Goldbrick”), regular equipment maintenance (“Fighting Tools”, “Gas”). Keeping military secrets was another theme, with “Spies” and “Censored”. Munro Leaf, who wrote “The Story of Ferdinand”, also wrote “The Chowhound” about not wasting food (both stories featuring bulls). 

Because the Snafu cartoons were only intended for an adult male military audience, there were a lot of subtle jokes and nudity. In “The Home Front”, Ted Geisel put in a joke just to see what the animator would do -“(In Alaska) it was so cold, it would freeze the nuts off a jeep”. “Booby Traps” literally included … a booby trap! The Axis villains are drawn very unfavorably. Germans are big, overweight and dumb. The Japanese are tiny with huge glasses and even bigger teeth. The casual racism of that wartime era is jarring today.

The Snafu cartoons succeeded because they entertained while delivering their brief lessons on keeping secrets, cleaning gear or mess hall chores. They weren’t preachy, and Snafu sometimes learned his lessons by the end. As the war progressed and an Allied victory became more and more likely, the Snafu cartoons became a little drier. “A Few Quick Facts: Inflation” and “A Few Quick Facts: Fear” were done by the UPA animation studio with a different but still entertaining style. “Hot Spots” (by Hanna and Barbera, who started “the Flintstones”) shows us how to survive in Iran, with its camel melting heat. “In the Aleutians” looks at the mud, wind and rain of the Alaskan island chain. When the war ended, so did the series – the last one, “Private SNAFU Presents Seaman TARFU” (written by Hank Ketcham, of “Dennis the Menace” fame) was never released. 

The Warner studios churned out a new Private Snafu cartoon every month (occasionally with help from a rival studio, like UPA and MGM) through the end of the war. With a total cost of only $300,000, these were some of the cheapest weapons, and certainly among the funniest. 

We hope you enjoy these Private Snafu cartoons!"
