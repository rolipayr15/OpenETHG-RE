using System;
using System.Text;
using UnityEngine;

public static class DeviceNetworkInfo
{
	private const int TransportCellular = 0;

	private const int TransportWifi = 1;

	private const int TransportEthernet = 3;

	private const int TransportVpn = 4;

	public static string Describe()
	{
		return null;
	}

	private static void Section(StringBuilder sb, string label, Func<string> describe)
	{
	}

	private static string DescribeActiveNetwork(AndroidJavaObject activity)
	{
		return null;
	}

	private static string Transport(AndroidJavaObject capabilities)
	{
		return null;
	}

	private static bool HasGlobalIpv6(AndroidJavaObject link)
	{
		return false;
	}

	private static string DescribeLinkProxy(AndroidJavaObject link)
	{
		return null;
	}

	private static string DescribePrivateDns(AndroidJavaObject link)
	{
		return null;
	}

	private static string DescribeDnsServers(AndroidJavaObject link)
	{
		return null;
	}

	private static string DescribeCarrier(AndroidJavaObject activity)
	{
		return null;
	}

	private static string DescribeJavaProxy()
	{
		return null;
	}

	private static string HostPort(AndroidJavaClass system, string scheme)
	{
		return null;
	}

	private static string Safe(Func<string> describe)
	{
		return null;
	}

	private static string Or(string value)
	{
		return null;
	}
}
