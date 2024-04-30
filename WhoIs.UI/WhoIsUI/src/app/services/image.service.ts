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


  getFaceImageAsBlob(imageId: string): Observable<HttpResponse<Blob>> {
    return this.httpClient.post(`${this.apiUri}/get-face-image/${imageId}`, null, { responseType: 'blob', observe: 'response' })
  }


}
