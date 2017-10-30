project(measure_cpp)

if(MSVC)
    message(SEND_ERROR "Not Support msvc by this CMakeLists.txt")
else()
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -std=c++11")
endif()

add_executable(measure_cpp measure_cpp.cpp speedtest.cpp fine_timer.h)
