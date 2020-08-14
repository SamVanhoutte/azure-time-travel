import json
import numpy as np
import pandas as pd
import os
import tensorflow as tf
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Activation
from tensorflow.keras.optimizers import SGD
from tensorflow.keras.layers import Dense, Dropout, Flatten
from tensorflow.keras.layers import Conv2D, MaxPooling2D
from tensorflow.keras.layers import Input, Dense, Conv2D, MaxPooling2D, UpSampling2D
from tensorflow.keras.models import Model
from tensorflow.keras.preprocessing import image
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout, Activation, BatchNormalization
from tensorflow.keras.applications.vgg19 import VGG19
from tensorflow.keras.applications.vgg19 import preprocess_input, decode_predictions
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.losses import categorical_crossentropy, binary_crossentropy, cosine_similarity
from tensorflow.keras import backend as K
from tensorflow.keras.callbacks import EarlyStopping, ModelCheckpoint


from tensorflow.keras.layers import Input, Dense, Conv2D, MaxPooling2D, UpSampling2D, Conv2DTranspose
from tensorflow.keras.models import Model


from azureml.core.model import Model
from inference_schema.schema_decorators import input_schema, output_schema
from inference_schema.parameter_types.numpy_parameter_type import NumpyParameterType

series_model = None

def init():
    global series_model

    series_model_root = Model.get_model_path('EngineFailurePrediction')
    print('Series Model root:', series_model_root)
    #series_model_file = os.path.join(series_model_root, 'model')
    #print('Series Model file:', series_model_file)
    series_model = tf.keras.models.load_model(series_model_root)
    series_model.compile(loss = ('binary_crossentropy'), optimizer='adam', metrics=['acc'])
    print(series_model.summary())


input_sample = np.random.rand(1,30,24)
output_sample = np.array([0])


@input_schema('data', NumpyParameterType(input_sample, enforce_shape=False))
@output_schema(NumpyParameterType(output_sample))
def run(data):
    print(type(data))

    data = np.array(data)

    log_data({"data shape": str(data.shape)})

    # If one sample is given, we'll reshape to have multiple dimensions
    if(len(data.shape)==2):
        log_data({"message": "Reshaping to 3D array"})
        data = data.reshape(1, data.shape[0], data.shape[1])

    if(len(data.shape)!=3):
        log_data({"exception": "An array of shape (n, 30, 24) is expected as input."})
        raise ValueError("An array of shape (n, 30, 24) is expected as input.")    
    if(data.shape[2] != 24):
        log_data({"exception": "The time windows should contain 24 features"})
        raise ValueError("The time windows should contain 24 features")
    if(data.shape[1] != 30):
        log_data({"message": "Padding with zeroed samples"})
        data = np.pad(data, ((0, 0),(30 - data.shape[1], 0),(0, 0)), 'constant')

    # make prediction
    failure_expected = series_model.predict(data)
    log_data({"predictions": str(failure_expected)})

    return failure_expected.tolist()

def log_data(logging_data: dict):
    print(json.dumps(logging_data))
