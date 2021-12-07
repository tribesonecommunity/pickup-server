//
// Access Functions
//
	
$accessLevel::Count = 0;
function addAdminLevel(%name)
{
	//if it already exists, reset the list
	for (%i=0; %i<$accessLevel::Count; %i++)
	{
		if ($accessLevel::[%i] == %name)
			$accessLevel::Count = 0;
	}

	$accessLevel::[$accessLevel::Count] = %name;
	$accessLevel::[%name] = $accessLevel::Count;
	$accessLevel::Count++;
}

function getAdminLevel(%name)
{
	return $accessLevel::[%name];
}


function zadmin::setCurrentAdminLevel(%name)
{
	$zadmin::addAdminHelperLevel = getAdminLevel(%name);
}


$zadmin::Admin = 0;
function zadmin::addAdmin(%name, %password)
{
	if ($zadmin::addAdminHelperLevel == "")
		$zadmin::addAdminHelperLevel = 0;

	$zadmin::admins[%password] = 1;
	$zadmin::admins[%password, level] = $zadmin::addAdminHelperLevel;
	$zadmin::admins[%password, name] = %name;
	
	$zadmin::_admins[ $zadmin::Admin++ -1 ] = %name @ " : " @ %password @ " : " @ $zadmin::addAdminHelperLevel;
}


//
// Global Spammer List
//

$zadmin::GlobalSpam::Names = 0;
$zadmin::GlobalSpam::IPs = 0;
function addGlobalSpammer(%name){$zadmin::GlobalSpam::Name[ $zadmin::GlobalSpam::Names++ -1 ] = %name;}
function addGlobalSpammerIP(%ip){$zadmin::GlobalSpam::IP[ $zadmin::GlobalSpam::IPs++ -1 ] = "IP:" @ %ip;}

$zadmin::MegaSpam::Names = 0;
$zadmin::MegaSpam::IPs = 0;
function addMegaSpammer(%name){$zadmin::MegaSpam::Name[ $zadmin::MegaSpam::Names++ -1 ] = %name;}
function addMegaSpammerIP(%ip){$zadmin::MegaSpam::IP[ $zadmin::MegaSpam::IPs++ -1 ] = "IP:" @ %ip;}

exec("zadmin.config.cs");
exec("zadmin.passwords.cs");