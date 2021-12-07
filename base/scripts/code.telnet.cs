function Telnet::PermanentBan(%client)
{
    %ip = Client::getTransportAddress(%bannedClient);
	%truncatedIP = parseIP(%bannedClient, %numWords, %stringSize, false);

	$IPBan[$IPBanCount++] = format(%truncatedIP, 20) @ format (%ip, 26) @ Client::getName(%bannedClient) @ " permanently banned by Web Console.";

	logEntry(-2, "(Web Console) permanently banned", %client, "@");
    //export("IPBan" @ $IPBanCount, "config\\" @ $zAdminBanLogFile, true);
	
    Net::kick(%client, $permaBanMessage);
    BanList::addAbsolute();
    BanList::add(%ip, 1800);
}


function Telnet::echoBanlist()
{
	exec($zAdminBanLogFile);

	Telnet::echoBanlistHelper(0);
}

function Telnet::echoBanlistHelper(%index)
{
	for (%i = %index; %i < %index + 50; %i++)
	{
		if ($IPBan[%i] != "")
		{
			$IPBanCount = %i;
			echo("@@@ " @ $IPBan[%i]);
		}
	}
	
	if (%index < 900)
		schedule("Telnet::echoBanlistHelper(" @ %index+50 @ ");", 0.1);
	else
		echo("@@@ end.");
}

function Telnet::echoSADList()
{
	for (%i = 0; %i < $zadmin::Admin; %i++)
	{
		echo("@@@ " @ $zadmin::_admins[%i]);
	}
	echo("@@@ end.");
}


function Telnet::exportBanlist()
{
	//export("IPBan*", "config\\" @ $zAdminBanLogFile, false);
}