#
#	Makefile for PrimMesher 
#	See http://forge.opensimulator.org/gf/project/primmesher
#
#	Release and Debug files are not placed in {obj,bin}/{Release,Debug}
#	by default to avoid clashing with the output from other build tools.
#	Use "make merge_all" to merge output with {obj,bin} if desired.
#

MCS	= gmcs

SYS_LIB	=					\
	-r:System.Drawing.dll			\
	-r:System.Drawing.Design		\
	-r:mscorlib.dll

RELEASE_DLL	=				\
	PrimMesher.dll				\
	Properties/AssemblyInfo.dll

DEBUG_DLL	=				\
	Debug/PrimMesher.dll			\
	Debug/AssemblyInfo.dll

all:				Debug $(RELEASE_DLL) $(DEBUG_DLL)

Debug:
	mkdir -p Debug

PrimMesher.dll:			PrimMesher.cs SculptMesh.cs
	$(MCS) $(SYS_LIB) PrimMesher.cs SculptMesh.cs -t:library

Debug/PrimMesher.dll:		PrimMesher.cs SculptMesh.cs
	$(MCS) $(SYS_LIB) PrimMesher.cs SculptMesh.cs -t:library -debug -out:$@

Properties/AssemblyInfo.dll:	Properties/AssemblyInfo.cs
	$(MCS) $< -t:library

Debug/AssemblyInfo.dll:		Properties/AssemblyInfo.cs
	$(MCS) $< -t:library -debug -out:$@

merge_release:			all
	cp -p $(RELEASE_DLL) obj/Release/
	cp -p $(RELEASE_DLL) bin/Release/

merge_debug:			all
	cp -p Debug/* obj/Debug/
	cp -p Debug/* bin/Debug/

merge_all:			merge_release merge_debug

clean:
	rm -f $(RELEASE_DLL)
	rm -f $(DEBUG_DLL) Debug/*.dll.mdb
