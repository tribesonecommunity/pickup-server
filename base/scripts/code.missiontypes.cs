// -------------------------------------------------
// declare the mission types here - 'cdTrack' is needed for
// all mission types
// -------------------------------------------------
 
$missionTypes[0, type]        = "CTF";
$missionTypes[0, description] = "Capture the Flag";
$missionTypes[0, minTeams]    = 2;
$missionTypes[0, maxTeams]    = 8;
$missionTypes[0, varName, 0]  = winCaps;
$missionTypes[0, varDesc, 0]  = "Flag caps to win";
$missionTypes[0, varVal, 0]   = 5;
$missionTypes[0, varName, 1]  = cdTrack;
$missionTypes[0, varDesc, 1]  = "CD Track";
$missionTypes[0, varVal, 1]   = 2;
$missionTypes[0, varName, 2]  = cdMode;
$missionTypes[0, varDesc, 2]  = "CD Play Mode";
$missionTypes[0, varVal, 2]   = 1;


// -------------------------------------------------
// create all the variables needed
// -------------------------------------------------

for(%i = 0; $missionTypes[%i, type] != ""; %i++)
   for(%j = 0; $missionTypes[%i, varName, %j] != ""; %j++)
      eval("$" @ $missionTypes[%i, type] @ "::" @ $missionTypes[%i, varName, %j] @ "=" @ $missionTypes[%i, varVal, %j] @ ";");

// -------------------------------------------------
// create functions - add stuff to the mission
// -------------------------------------------------


function Mission::CTF::create(%numTeams)
{
   // add a flag
   for(%i = 0; %i < %numTeams; %i++)
   {
      // add a 'base' group for the flag
      %base = newObject(Base, SimGroup);
      addToSet("MissionGroup\\Teams\\team" @ %i, %base);

      // add the flag
      %flag = newObject(Flag @ %i, Item, Flag, 1, false);
      %flag.scoreValue = 1;
      GameBase::setMapName(%flag, "Flag " @ (%i + 1));
      addToSet(%base, %flag);
   }
   
   // add lines to the mission file...
   addExportText("$teamScoreLimit = " @ $CTF::winCaps @ ";");
   addExportText("exec(objectives);");
   addExportText("$Game::missionType = \"CTF\";");
}
