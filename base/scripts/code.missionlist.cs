function MissionList::clear()
{
   $MLIST::Count = 0;
   $MLIST::TypeCount = 1;
   $MLIST::Type[0] = "All Types";
   $MLIST::MissionList[0] = "";
}  

function MissionList::addMission(%mission)
{
   $MDESC::Type = "";
   $MDESC::Name = "";
   $MDESC::Text = "";

   if (String::findSubStr(%mission,".dsc") == -1)
      %mission = %mission @ ".dsc";
   exec(%mission);

   if($MDESC::Type == "")
      return false;

   for(%i = 0; %i < $MLIST::TypeCount; %i++) {
      if($MLIST::Type[%i] == $MDESC::Type)
         break;
   }
   if(%i == $MLIST::TypeCount) {
      $MLIST::Type[%i] = $MDESC::Type;
      $MLIST::TypeCount++;
      $MLIST::MissionList[%i] = "";
   }
   %ct = $MLIST::Count;
   $MLIST::Count++;

   $MLIST::EType[%ct] = $MDESC::Type;
   $MLIST::EName[%ct] = File::getBase(%mission);
   $MLIST::EText[%ct] = $MDESC::Text;
   if($MDESC::Type != "Training")
      $MLIST::MissionList[0] = %ct @ " " @ $MLIST::MissionList[0];
   $MLIST::MissionList[%i] = %ct @ " " @ $MLIST::MissionList[%i];

   return true;
}

function MissionList::build()
{
   MissionList::clear();

   %file = File::findFirst("missions\\*.dsc");
   while(%file != "") {
      MissionList::addMission(%file);
      %file = File::findNext("missions\\*.dsc");
   }
}

function MissionList::initNextMission()
{
   for(%type = 1; %type < $MLIST::TypeCount; %type++) {
      %prev = getWord($MLIST::MissionList[%type], 0);
      %ml = $MLIST::MissionList[%type] @ %prev;
      %prevName = $MLIST::EName[%prev];
      for(%i = 1; (%mis = getWord(%ml, %i)) != -1; %i++) {
         %misName = $MLIST::EName[%mis];
         $nextMission[%prevName] = %misName;
         %prevName = %misName;
      }
   }
}


// Go ahead and build the list
MissionList::build();
MissionList::initNextMission();