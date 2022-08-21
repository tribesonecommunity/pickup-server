//saving
function AntiSkip::SaveFlag(%team)
{
	exportObjectToScript($TeamFlag[%team], "temp\\teamflag"@%team@".cs");

	$AntiSkip::Object[%team] = GetGroup($TeamFlag[%team]);
	$AntiSkip::Name[%team] = Object::GetName($TeamFlag[%team]);
}

function AntiSkip::RestoreFlag(%team)
{
	if ($AntiSkip::Object[%team] == "")
		return $teamflag[%team];
	
	//copy original info
	%flag = $teamflag[%team];
	%copyatHome = %flag.atHome;
	%copypickupSequence = %flag.pickupSequence;
	%copycarrier = %flag.carrier;
	%copyholdingTeam = %flag.holdingTeam;
	%copyenemyCaps = %flag.enemyCaps;
	%copyholdingTeam = %flag.holdingTeam;
	for (%i=0; %i<7; %i++)
		%copycaps[%i] = %flag.caps[%i];

	DeleteObject($teamflag[%team]);
	exec("teamflag"@%team@".cs");

	//put original info back
	%flag = nametoid($AntiSkip::Name[%team]);
	%flag.atHome = %copyatHome;
	%flag.pickupSequence = %copypickupSequence;
	%flag.carrier = %copycarrier;
	%flag.holdingTeam = %copyholdingTeam;
	%flag.enemyCaps = %copyenemyCaps;
	%flag.holdingTeam = %copyholdingTeam;
	for (%i=0; %i<7; %i++)
		%flag.caps[%i] = %copycaps[%i];

	$TeamFlag[%team] = %flag;
	AddToSet($AntiSkip::Object[%team], %flag);

	// Add to the objecives
	addtoset(nameToId("MissionCleanup/ObjectivesSet"), %flag);
	
	return %flag;
}