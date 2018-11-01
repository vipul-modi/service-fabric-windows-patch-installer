---
services: service-fabric
platforms: dotnet, windows
author: ninzavivek
---

## About this Patch
This patch queries for IPv4 interfaces on a cluster node that have MTU size set less than 1500 bytes. If it finds one, it will reset the MTU value to 1500 for the interface. This is done in order to match the MTU size set on an interface programmed inside a Windows Container (NAT Networking mode). MTU size mismatch leads to fragmentation which could cause choppy connections in some cases.

Please note, this patch is only supposed to deployed on Service Fabric Clusters that have L2 tunnel network setup with only NAT networking mode configured Windows containers running on the nodes.    
