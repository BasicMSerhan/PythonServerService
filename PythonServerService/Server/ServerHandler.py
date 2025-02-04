print("Loading Imports!")

import pickle
import pymssql
import numpy as np
import pandas as pd



import warnings
warnings.filterwarnings('ignore')
from scipy.sparse import hstack
from sklearn.svm import LinearSVC
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler, OneHotEncoder
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline
from sklearn.ensemble import RandomForestClassifier
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import classification_report, accuracy_score
from sklearn.decomposition import PCA
from sklearn.preprocessing import OneHotEncoder, StandardScaler
from sklearn.decomposition import TruncatedSVD
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import accuracy_score

import datetime

from http.server import HTTPServer, BaseHTTPRequestHandler
from urllib.parse import parse_qs
import json

SERVER_PORT = 7001

SeasonMapping = {'1':'SUMMER','2': 'WINTER', '3': 'YEARLY', '4' : 'WINTER'}

print("Done Loading Imports!")

print("Loading Prediction Model!")

# Model Load
tfLinearSVC=pickle.load(open('tfLinearSVC.pkl','rb'))
modelLinearSVC=pickle.load(open('modelLinearSVC.pkl','rb'))
tfLogisticReg=pickle.load(open('tfLogisticReg.pkl','rb'))
modelLogisticReg=pickle.load(open('modelLogisticReg.pkl','rb'))

tfLinearSVC_pred=TfidfVectorizer(sublinear_tf = True, min_df=10,ngram_range=(1,2),stop_words='english',vocabulary=tfLinearSVC.vocabulary_)
tfLogisticReg_pred=TfidfVectorizer(sublinear_tf = True, min_df=10,ngram_range=(1,2),stop_words='english',vocabulary=tfLogisticReg.vocabulary_)

print("Done Loading Prediction Model!")

class Serv(BaseHTTPRequestHandler):

    def do_POST(self):
        response = "Access Denied!"
        self.send_response(403)
        self.end_headers()
        self.wfile.write(bytes(response, 'utf-8'))

    def do_GET(self):
        getDict = {}
        path = self.path
        if '?' in path:
            path, tmp = path.split('?', 1)
            getDict = parse_qs(tmp)
        if path == '/PredictUPCSale':
            print ("Received GET Parameters:", getDict)

            if not getDict:
                response = "Access Denied!"
                self.send_response(403)
            else:
                if not getDict["DataArray"]:
                    response = "Access Denied!"
                    self.send_response(403)
                else:
                    try:
                        data = json.loads(getDict["DataArray"][0])

                        print("Running Prediction For Data:", json.dumps(data))

                        start_time = datetime.datetime.now()

                        new_data = np.array(data)
                        #new_data_mapped = [SeasonMapping.get(str(x).upper(), str(x).upper()) for x in new_data]
                        new_data_mapped = [str(x).upper() for x in new_data]
                        new_data_mapped[10] = SeasonMapping[new_data_mapped[10]]
                        concatenated_string = " ".join(new_data_mapped)

                        FeaturesLinearSVC=tfLinearSVC_pred.fit_transform([concatenated_string])
                        FeaturesLogisticReg=tfLogisticReg_pred.fit_transform([concatenated_string])

                        pred_LinearSVC = modelLinearSVC.predict(FeaturesLinearSVC)
                        pred_LogisticReg = modelLogisticReg.predict(FeaturesLogisticReg)

                        end_time = datetime.datetime.now()

                        print("Done Running Prediction, Took:", ((end_time - start_time).total_seconds() * 1000), "ms.")
                        response = str(pred_LinearSVC[0]) + "," + str(pred_LogisticReg[0])
                        self.send_response(200)
                    except Exception as ex:
                        response = "Internal Server Error!\n" + repr(ex)
                        print("An Error Occured Running The Request:", response)
                        self.send_response(500)
        else: 
            response = "MS The Star, Python Server Active!"
            self.send_response(200)
        self.end_headers()
        self.wfile.write(bytes(response, 'utf-8'))

httpd = HTTPServer(('localhost',SERVER_PORT),Serv)
print ("Launching HTTP Server On Port", SERVER_PORT)
httpd.serve_forever()