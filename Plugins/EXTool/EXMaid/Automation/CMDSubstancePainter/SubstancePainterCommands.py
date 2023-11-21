import sys
import lib_remote
import socket

def NotifyUnity(port,message):
    client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    print('start connect')
    client.connect(('localhost', port))
    print('connect success')
    client.sendall(message.encode())
    print('send message to the unity:'+message)
    client.close()
    return;

def CreateRemote():
    Remote = lib_remote.RemotePainter() 
    Remote.checkConnection() 
    return Remote


def OpenSpp(sppPath):   
    Remote = CreateRemote()
    # import the substance_painter.project module to make 
    # its API available to us 
    Remote.execScript( 'import substance_painter.project as spp_project', "python" ) 

    CommandSetSppPath = 'spp_file_path = "{0}"'.format( sppPath )  
    Remote.execScript( CommandSetSppPath, "python" ) 
    Remote.execScript( 'spp_project.open(spp_file_path)', "python" ) 
    return;
    
    
def ReloadModel(modelFilePath):
    Remote = CreateRemote()
    Remote.execScript( 'import substance_painter.project as sp_project', "python" ) 
    CommandSetModelPath = 'modelFilePath = "{0}"'.format( modelFilePath )  
    Remote.execScript( CommandSetModelPath, "python" )
    # create MeshReloadingSettings
    # keep camera and preserve strokes
    CommandSetMeshReloadingSettings = 'mesh_reloading_settings = ' \
                                      'sp_project.MeshReloadingSettings(' \
                                      'import_cameras = True, ' \
                                      'preserve_strokes = True )'
    Remote.execScript( CommandSetMeshReloadingSettings, "python" )
    
    CommandDefOnReloadResult =' on_reload_result = lambda value: print("Reload Result = {0}".format(str(value))';
    Remote.execScript( CommandDefOnReloadResult, "python" )
    
    Remote.execScript('sp_project.reload_mesh(modelFilePath, mesh_reloading_settings, None)', "python" )
    return;

def ImportTexture(texFilePath,texName):
    Remote = CreateRemote()
    Remote.execScript( 'import substance_painter.resource as sp_resource', "python" )
    
    CommandSetTexturePath = 'texture_file = "{0}"'.format( texFilePath )
    Remote.execScript( CommandSetTexturePath, "python" )

    Remote.execScript( 'usage = sp_resource.Usage("texture")', "python" )
    
    CommandSetNamePath = 'name = "{0}"'.format( texName )
    Remote.execScript( CommandSetNamePath, "python" )

    Remote.execScript( 'group = "/project/"', "python" )
    Remote.execScript( 'resource = sp_resource.import_project_resource(texture_file,usage,name,group)', "python" )
    return;


def UpdateProjectMeshMapTexture(MatName,texType,texName):
    Remote = CreateRemote()
    CommandImportMoudle = 'import substance_painter.textureset as sp_texSet;' \
                          'import substance_painter.project as sp_project;' \
                          'import substance_painter.resource as sp_resource;'
    Remote.execScript(CommandImportMoudle, "python" )

    CommandGetTexSet = 'texture_set = sp_texSet.TextureSet.from_name("{0}")'.format( MatName )
    Remote.execScript(CommandGetTexSet, "python" )

    CommandSetUasge = 'meshMapUsage = sp_texSet.MeshMapUsage.{0}'.format( texType )
    Remote.execScript(CommandSetUasge, "python" )

    CommandGetResourceId = 'resourceId = sp_resource.ResourceID.from_project("{0}")'.format( texName )
    Remote.execScript(CommandGetResourceId, "python" )

    Remote.execScript('texture_set.set_mesh_map_resource(meshMapUsage , resourceId )', "python" )
    return;


def ExportTextures(exportPath,MatName):
    Remote = CreateRemote()
    CommandImportMoudle = 'import substance_painter.export as sp_export;' \
                          'import substance_painter.resource as sp_resource;'
    Remote.execScript(CommandImportMoudle, "python" )

    CmdSetExportPresetUrl = 'export_preset_url = sp_resource.ResourceID(context="starter_assets", name="Unity Universal Render Pipeline (Metallic Standard)").url()';
    Remote.execScript(CmdSetExportPresetUrl, "python" )

    Remote.execScript('exportPath = "{0}"'.format( exportPath ), "python" )
    Remote.execScript('MatName = "{0}"'.format( MatName ), "python" )
    
    Remote.execScript('export_config = {}', "python" )
    
    Remote.execScript('export_config["exportPath"] = exportPath', "python" )
    Remote.execScript('export_config["exportShaderParams"] = False', "python" )
    Remote.execScript('export_config["defaultExportPreset"] = export_preset_url', "python" )
    Remote.execScript('export_config["exportList"] = [{"rootPath": MatName}]', "python" )
    Remote.execScript('export_config["exportParameters"] = [{"parameters":{"fileFormat" : "png","bitDepth" : "8","dithering": True,"paddingAlgorithm": "infinite"}}]', "python" )

    Remote.execScript('export_result = sp_export.export_project_textures(export_config)', "python" )
    Status = Remote.execScript('export_result.status', "python" )
    StatusSuccess = Remote.execScript('sp_export.ExportStatus.Success', "python" )

    msg = None;
    if(Status == StatusSuccess):
        msg = 'SubPainterExportComplete';
        Remote.execScript('paths = export_result.textures[(MatName,"")]', "python");
        path1 = Remote.execScript('paths[0]', "python");
        msg = msg + '||' + path1
        path2 = Remote.execScript('paths[1]', "python");
        msg = msg + '||' + path2
        path3 = Remote.execScript('paths[2]', "python");
        msg = msg + '||' + path3
    else:
        msg = 'SubPainterExportComplete_Fail'
            
    return msg;

def CloseProject():
    Remote = CreateRemote()
    Remote.execScript( 'import substance_painter.project as sp_project', "python" )
    Remote.execScript( 'sp_project.close()', "python" )
    return;