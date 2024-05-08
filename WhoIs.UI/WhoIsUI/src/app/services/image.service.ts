import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment'
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FaceInfo } from '../models/face-info.model';
import { ApiResponse } from '../models/api.response.model';

@Injectable({
  providedIn: 'root'
})
export class ImageService {

  apiUri: string = '';
  constructor(private httpClient: HttpClient) {
    this.apiUri = environment.apiUri;
  }

  getFaceInfos(): Observable<ApiResponse<FaceInfo[]>> {
    return this.httpClient.get<ApiResponse<FaceInfo[]>>(`${this.apiUri}/get-face-ids`);
  }

  getImageAsBlob(imageId: string, isFaceImage: boolean): Observable<HttpResponse<Blob>> {
    return this.httpClient.post(`${this.apiUri}/get-image/${imageId}?isFaceImage=${isFaceImage}`, null, { responseType: 'blob', observe: 'response' })
  }

  updateFaceName(faceId: string, name: string): Observable<ApiResponse<boolean>> {
    return this.httpClient.put<ApiResponse<boolean>>(`${this.apiUri}/update-face-name/${faceId}/${name}`, null)
  }

  getImagePathsWithFaceName(searchText: string): Observable<ApiResponse<string[]>> {
    return this.httpClient.get<ApiResponse<string[]>>(`${this.apiUri}/imageIds/face/${searchText}`);
  }

}
